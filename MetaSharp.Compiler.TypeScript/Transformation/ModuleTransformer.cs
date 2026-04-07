using MetaSharp.Compiler;
using MetaSharp.TypeScript.AST;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaSharp.Transformation;

/// <summary>
/// Transforms a static C# class into top-level TypeScript functions instead of a class.
/// Two flavors are supported on the same input:
///
/// <list type="bullet">
///   <item>
///     Plain static methods on a <c>[ExportedAsModule]</c> static class → emitted as
///     standalone <c>export function</c>s.
///   </item>
///   <item>
///     Classic-style C# extension methods (first param tagged <c>this</c>) and C# 14
///     <c>extension(Receiver receiver) { ... }</c> blocks → also emitted as standalone
///     functions, with the receiver becoming the first parameter.
///   </item>
/// </list>
///
/// Both forms keep the original method bodies (transformed via the
/// <see cref="ExpressionTransformer"/>) and are visible to consumers as ordinary
/// importable functions rather than members of a class.
/// </summary>
public sealed class ModuleTransformer(TypeScriptTransformContext context)
{
    private readonly TypeScriptTransformContext _context = context;

    public void Transform(INamedTypeSymbol type, List<TsTopLevel> statements)
    {
        // Process direct members (classic extension methods, plain static functions)
        foreach (var member in type.GetMembers())
        {
            if (SymbolHelper.HasIgnore(member)) continue;

            switch (member)
            {
                case IMethodSymbol { MethodKind: MethodKind.Ordinary } method:
                    var func = TransformModuleFunction(method);
                    if (func is not null) statements.Add(func);
                    break;

                // Extension properties on classic style (parameters via Roslyn)
                case IPropertySymbol prop when prop.Parameters.Length > 0:
                    var propFunc = TransformExtensionProperty(prop);
                    if (propFunc is not null) statements.Add(propFunc);
                    break;
            }
        }

        // Process C# 14 extension blocks via syntax tree
        // (Roslyn exposes them as nested anonymous types with TypeKind=Extension,
        //  but it's simpler to walk the syntax directly)
        foreach (var syntaxRef in type.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            foreach (var node in syntax.DescendantNodes())
            {
                if (node.Kind().ToString() != "ExtensionBlockDeclaration") continue;
                TransformExtensionBlock(node, statements);
            }
        }
    }

    /// <summary>
    /// Transforms a C# 14 extension block syntax into top-level functions.
    /// The block syntax is: <c>extension(Type receiver) { members... }</c>.
    /// </summary>
    private void TransformExtensionBlock(SyntaxNode extensionBlock, List<TsTopLevel> statements)
    {
        // ExtensionBlockDeclarationSyntax has ParameterList and Members
        var paramListProp = extensionBlock.GetType().GetProperty("ParameterList");
        var membersProp = extensionBlock.GetType().GetProperty("Members");
        if (paramListProp?.GetValue(extensionBlock) is not ParameterListSyntax paramList) return;
        if (membersProp?.GetValue(extensionBlock) is not SyntaxList<MemberDeclarationSyntax> members) return;
        if (paramList.Parameters.Count == 0) return;

        var receiverParamSyntax = paramList.Parameters[0];
        var semanticModel = _context.Compilation.GetSemanticModel(extensionBlock.SyntaxTree);

        var receiverName = TypeScriptNaming.ToCamelCase(receiverParamSyntax.Identifier.Text);
        var receiverTypeSymbol = receiverParamSyntax.Type is null
            ? null
            : semanticModel.GetTypeInfo(receiverParamSyntax.Type).Type;
        if (receiverTypeSymbol is null) return;
        var receiverType = TypeMapper.Map(receiverTypeSymbol);
        var receiverParam = new TsParameter(receiverName, receiverType);

        var exprTransformer = _context.CreateExpressionTransformer(semanticModel);

        foreach (var member in members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax methodSyntax:
                {
                    var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax) as IMethodSymbol;
                    if (methodSymbol is null) continue;
                    if (methodSymbol.DeclaredAccessibility != Accessibility.Public) continue;

                    var name = SymbolHelper.GetNameOverride(methodSymbol)
                        ?? TypeScriptNaming.ToCamelCase(methodSymbol.Name);
                    var returnType = TypeMapper.Map(methodSymbol.ReturnType);
                    var parameters = new List<TsParameter> { receiverParam };
                    parameters.AddRange(methodSymbol.Parameters.Select(p =>
                        new TsParameter(TypeScriptNaming.ToCamelCase(p.Name), TypeMapper.Map(p.Type))));

                    var body = exprTransformer.TransformBody(methodSyntax.Body, methodSyntax.ExpressionBody,
                        isVoid: methodSymbol.ReturnsVoid);
                    statements.Add(new TsFunction(name, parameters, returnType, body, Exported: true));
                    break;
                }
                case PropertyDeclarationSyntax propSyntax:
                {
                    var propSymbol = semanticModel.GetDeclaredSymbol(propSyntax) as IPropertySymbol;
                    if (propSymbol is null) continue;
                    if (propSymbol.DeclaredAccessibility != Accessibility.Public) continue;

                    var name = SymbolHelper.GetNameOverride(propSymbol)
                        ?? TypeScriptNaming.ToCamelCase(propSymbol.Name);
                    var returnType = TypeMapper.Map(propSymbol.Type);
                    var parameters = new List<TsParameter> { receiverParam };

                    IReadOnlyList<TsStatement> body;
                    if (propSyntax.ExpressionBody is not null)
                        body = [new TsReturnStatement(exprTransformer.TransformExpression(propSyntax.ExpressionBody.Expression))];
                    else if (propSyntax.AccessorList is not null)
                    {
                        var getAccessor = propSyntax.AccessorList.Accessors
                            .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
                        if (getAccessor is null) continue;
                        body = exprTransformer.TransformBody(getAccessor.Body, getAccessor.ExpressionBody);
                    }
                    else continue;

                    statements.Add(new TsFunction(name, parameters, returnType, body, Exported: true));
                    break;
                }
            }
        }
    }

    private TsFunction? TransformExtensionProperty(IPropertySymbol prop)
    {
        if (prop.DeclaredAccessibility != Accessibility.Public) return null;
        if (prop.IsImplicitlyDeclared) return null;

        var name = SymbolHelper.GetNameOverride(prop) ?? TypeScriptNaming.ToCamelCase(prop.Name);
        var returnType = TypeMapper.Map(prop.Type);

        // The receiver parameter
        var parameters = prop.Parameters
            .Select(p => new TsParameter(TypeScriptNaming.ToCamelCase(p.Name), TypeMapper.Map(p.Type)))
            .ToList();

        // Get the getter body
        var getter = prop.GetMethod;
        if (getter is null) return null;

        var syntax = getter.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        var semanticModel = _context.Compilation.GetSemanticModel(syntax!.SyntaxTree);
        var exprTransformer = _context.CreateExpressionTransformer(semanticModel);

        IReadOnlyList<TsStatement> body;
        if (syntax is AccessorDeclarationSyntax accessor)
            body = exprTransformer.TransformBody(accessor.Body, accessor.ExpressionBody);
        else if (syntax is ArrowExpressionClauseSyntax arrow)
            body = [new TsReturnStatement(exprTransformer.TransformExpression(arrow.Expression))];
        else
            return null;

        return new TsFunction(name, parameters, returnType, body);
    }

    private TsFunction? TransformModuleFunction(IMethodSymbol method)
    {
        if (method.DeclaredAccessibility != Accessibility.Public) return null;
        if (method.IsImplicitlyDeclared) return null;
        if (TypeScriptNaming.HasEmit(method)) return null;
        // Skip property accessors — extension properties are handled via their associated property
        if (method.AssociatedSymbol is IPropertySymbol) return null;

        var syntaxNode = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        if (syntaxNode is null) return null;

        var name = SymbolHelper.GetNameOverride(method) ?? TypeScriptNaming.ToCamelCase(method.Name);
        var hasYield = syntaxNode.DescendantNodes().OfType<YieldStatementSyntax>().Any();
        var returnType = hasYield
            ? TypeMapper.MapForGeneratorReturn(method.ReturnType)
            : TypeMapper.Map(method.ReturnType);
        var isAsync = hasYield ? false : method.IsAsync;

        var parameters = method.Parameters
            .Select(p => new TsParameter(TypeScriptNaming.ToCamelCase(p.Name), TypeMapper.Map(p.Type)))
            .ToList();

        var semanticModel = _context.Compilation.GetSemanticModel(syntaxNode.SyntaxTree);
        var exprTransformer = _context.CreateExpressionTransformer(semanticModel);

        IReadOnlyList<TsStatement> body;
        if (syntaxNode is MethodDeclarationSyntax methodSyntax)
            body = exprTransformer.TransformBody(methodSyntax.Body, methodSyntax.ExpressionBody);
        else if (syntaxNode is ArrowExpressionClauseSyntax arrow)
            body = [new TsReturnStatement(exprTransformer.TransformExpression(arrow.Expression))];
        else
            return null;

        return new TsFunction(name, parameters, returnType, body, Exported: true, Async: isAsync,
            Generator: hasYield,
            TypeParameters: TypeTransformer.ExtractMethodTypeParameters(method));
    }
}
