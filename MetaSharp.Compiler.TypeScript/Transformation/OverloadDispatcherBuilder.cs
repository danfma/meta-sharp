using MetaSharp.Compiler;
using MetaSharp.TypeScript.AST;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaSharp.Transformation;

/// <summary>
/// Generates the overload-dispatching plumbing emitted for C# methods and constructors
/// that share a name. Both forms emit a single public entry point that takes
/// <c>...args: unknown[]</c> and routes at runtime to the right specialization based on
/// argument count + per-parameter type checks (provided by <see cref="TypeCheckGenerator"/>).
///
/// <list type="bullet">
///   <item>
///     <see cref="BuildConstructor"/> emits the dispatcher constructor + overload signatures
///     and inlines each constructor body inside a runtime-checked <c>if</c> branch.
///   </item>
///   <item>
///     <see cref="BuildMethod"/> emits the public dispatcher method plus one private fast-path
///     specialization per overload (each with the original body), and the dispatcher delegates
///     to the matching fast path with cast arguments.
///   </item>
/// </list>
/// </summary>
public sealed class OverloadDispatcherBuilder(TypeScriptTransformContext context)
{
    private readonly TypeScriptTransformContext _context = context;

    /// <summary>
    /// Generates a constructor with overload signatures and a runtime dispatcher body.
    /// Each branch inlines the body of the matching C# constructor (and a <c>super(...)</c>
    /// call when applicable).
    /// </summary>
    public TsConstructor BuildConstructor(
        INamedTypeSymbol type,
        List<IMethodSymbol> constructors,
        TsType? extendsType)
    {
        // Sort: most specific first (more params first, then by type specificity)
        var sorted = constructors.OrderByDescending(c => c.Parameters.Length).ToList();

        // Generate overload signatures
        var overloads = sorted.Select(ctor =>
        {
            var @params = ctor.Parameters
                .Select(p => new TsConstructorParam(
                    TypeScriptNaming.ToCamelCase(p.Name),
                    TypeMapper.Map(p.Type)))
                .ToList();
            return new TsConstructorOverload(@params);
        }).ToList();

        // Generate dispatcher body
        var body = new List<TsStatement>();

        foreach (var ctor in sorted)
        {
            var paramCount = ctor.Parameters.Length;

            TsExpression condition = new TsBinaryExpression(
                new TsPropertyAccess(new TsIdentifier("args"), "length"),
                "===",
                new TsLiteral(paramCount.ToString()));

            for (var i = 0; i < paramCount; i++)
            {
                var check = TypeCheckGenerator.GenerateForParam(
                    ctor.Parameters[i].Type, i,
                    _context.AssemblyWideTranspile, _context.CurrentAssembly);
                condition = new TsBinaryExpression(condition, "&&", check);
            }

            var assignStatements = new List<TsStatement>();

            // If extending, call super — try to resolve from syntax
            if (extendsType is not null)
            {
                var syntax = ctor.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                if (syntax is ConstructorDeclarationSyntax ctorSyntax && ctorSyntax.Initializer is not null
                    && ctorSyntax.Initializer.ThisOrBaseKeyword.Text == "base")
                {
                    var semanticModel = _context.Compilation.GetSemanticModel(ctorSyntax.SyntaxTree);
                    var exprTransformer = _context.CreateExpressionTransformer(semanticModel);
                    var superArgs = ctorSyntax.Initializer.ArgumentList.Arguments
                        .Select(a => exprTransformer.TransformExpression(a.Expression))
                        .ToList();
                    assignStatements.Add(new TsExpressionStatement(
                        new TsCallExpression(new TsIdentifier("super"), superArgs)));
                }
            }

            // Transform constructor body
            var ctorSyntaxNode = ctor.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            if (ctorSyntaxNode is ConstructorDeclarationSyntax ctorDecl && ctorDecl.Body is not null)
            {
                var semanticModel = _context.Compilation.GetSemanticModel(ctorDecl.SyntaxTree);
                var exprTransformer = _context.CreateExpressionTransformer(semanticModel);

                foreach (var stmt in ctorDecl.Body.Statements)
                {
                    assignStatements.Add(exprTransformer.TransformStatement(stmt));
                }
            }
            else if (ctorSyntaxNode is ConstructorDeclarationSyntax ctorExpr && ctorExpr.ExpressionBody is not null)
            {
                var semanticModel = _context.Compilation.GetSemanticModel(ctorExpr.SyntaxTree);
                var exprTransformer = _context.CreateExpressionTransformer(semanticModel);
                assignStatements.Add(new TsExpressionStatement(
                    exprTransformer.TransformExpression(ctorExpr.ExpressionBody.Expression)));
            }

            body.Add(new TsIfStatement(condition, assignStatements));
        }

        // Dispatcher constructor: (...args: unknown[])
        var dispatcherParams = new List<TsConstructorParam>
        {
            new("...args", new TsNamedType("unknown[]"))
        };

        return new TsConstructor(dispatcherParams, body, overloads);
    }

    /// <summary>
    /// Generates a public dispatcher method plus one private fast-path specialization per
    /// overload. The dispatcher's body branches on argument count + runtime type checks
    /// and delegates to the matching fast path with cast arguments.
    /// </summary>
    public IReadOnlyList<TsClassMember> BuildMethod(
        INamedTypeSymbol type, List<IMethodSymbol> methods)
    {
        var sorted = methods.OrderByDescending(m => m.Parameters.Length).ToList();
        var firstName = sorted[0];
        var name = SymbolHelper.GetNameOverride(firstName) ?? TypeScriptNaming.ToCamelCase(firstName.Name);
        var isStatic = firstName.IsStatic;
        var isAsync = sorted.Any(m => m.IsAsync);
        var accessibility = TypeTransformer.MapAccessibility(firstName.DeclaredAccessibility);

        // Determine a common return type (use unknown if they differ)
        var returnTypes = sorted.Select(m => TypeMapper.Map(m.ReturnType)).ToList();
        TsType commonReturn;
        if (returnTypes.All(t => t == returnTypes[0]))
        {
            commonReturn = returnTypes[0];
        }
        else if (isAsync)
        {
            commonReturn = new TsNamedType("Promise", [new TsNamedType("unknown")]);
        }
        else
        {
            commonReturn = new TsAnyType();
        }

        // Generate overload signatures (kept on the dispatcher for backward compat)
        var overloads = sorted.Select(m =>
        {
            var @params = m.Parameters
                .Select(p => new TsParameter(TypeScriptNaming.ToCamelCase(p.Name), TypeMapper.Map(p.Type)))
                .ToList();
            return new TsMethodOverload(@params, TypeMapper.Map(m.ReturnType));
        }).ToList();

        // Compute fast-path names (one per overload, unique within the group)
        var fastPathNames = ComputeFastPathNames(name, sorted);

        // Generate fast-path methods (specialized, one per overload, with the real body)
        var members = new List<TsClassMember>();
        for (var i = 0; i < sorted.Count; i++)
        {
            var method = sorted[i];
            var fastPathName = fastPathNames[i];
            var fastPathMethod = BuildFastPathMethod(method, fastPathName, isStatic);
            if (fastPathMethod is not null) members.Add(fastPathMethod);
        }

        // Generate dispatcher body that delegates to fast-paths
        var body = new List<TsStatement>();
        for (var i = 0; i < sorted.Count; i++)
        {
            var method = sorted[i];
            var fastPathName = fastPathNames[i];
            var paramCount = method.Parameters.Length;

            TsExpression condition = new TsBinaryExpression(
                new TsPropertyAccess(new TsIdentifier("args"), "length"),
                "===",
                new TsLiteral(paramCount.ToString()));

            for (var j = 0; j < paramCount; j++)
            {
                var check = TypeCheckGenerator.GenerateForParam(
                    method.Parameters[j].Type, j,
                    _context.AssemblyWideTranspile, _context.CurrentAssembly);
                condition = new TsBinaryExpression(condition, "&&", check);
            }

            // Build delegating call: this.fastPathName(args[0] as T0, …) (or ClassName.fastPathName for static)
            var callArgs = new List<TsExpression>();
            for (var j = 0; j < paramCount; j++)
            {
                var paramType = TypeMapper.Map(method.Parameters[j].Type);
                callArgs.Add(new TsCastExpression(
                    new TsIdentifier($"args[{j}]"),
                    paramType));
            }

            var receiver = isStatic
                ? (TsExpression)new TsIdentifier(type.Name)
                : new TsIdentifier("this");
            var delegateCall = new TsCallExpression(
                new TsPropertyAccess(receiver, fastPathName),
                callArgs);

            var branchStatements = new List<TsStatement>();
            if (method.ReturnsVoid)
            {
                branchStatements.Add(new TsExpressionStatement(delegateCall));
                branchStatements.Add(new TsReturnStatement());
            }
            else
            {
                branchStatements.Add(new TsReturnStatement(delegateCall));
            }

            body.Add(new TsIfStatement(condition, branchStatements));
        }

        // Add throw at the end for unmatched overloads
        body.Add(new TsThrowStatement(
            new TsNewExpression(new TsIdentifier("Error"),
                [new TsStringLiteral($"No matching overload for {name}")])));

        // Dispatcher params: ...args: unknown[]
        var dispatcherParams = new List<TsParameter>
        {
            new("...args", new TsNamedType("unknown[]"))
        };

        members.Add(new TsMethodMember(name, dispatcherParams, commonReturn, body,
            Static: isStatic, Async: isAsync, Accessibility: accessibility, Overloads: overloads));

        return members;
    }

    /// <summary>
    /// Generates a specialized method for one specific overload (the fast-path that the
    /// dispatcher delegates to once a match is detected).
    /// </summary>
    private TsMethodMember? BuildFastPathMethod(
        IMethodSymbol method, string fastPathName, bool isStatic)
    {
        var syntax = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
        if (syntax is null) return null;

        var semanticModel = _context.Compilation.GetSemanticModel(syntax.SyntaxTree);
        var exprTransformer = _context.CreateExpressionTransformer(semanticModel);
        if (!method.IsStatic) exprTransformer.SelfParameterName = "this";

        var parameters = method.Parameters
            .Select(p => new TsParameter(TypeScriptNaming.ToCamelCase(p.Name), TypeMapper.Map(p.Type)))
            .ToList();
        var returnType = TypeMapper.Map(method.ReturnType);
        var body = exprTransformer.TransformBody(syntax.Body, syntax.ExpressionBody, isVoid: method.ReturnsVoid);

        return new TsMethodMember(
            fastPathName,
            parameters,
            returnType,
            body,
            Static: isStatic,
            Async: method.IsAsync,
            Accessibility: TsAccessibility.Private,  // fast paths are internal — dispatcher is the public API
            TypeParameters: TypeTransformer.ExtractMethodTypeParameters(method)
        );
    }

    /// <summary>
    /// Computes a unique fast-path name for each overload in a group.
    /// Strategy: name + capitalized param names (e.g., addXY). On conflict, append type names.
    /// </summary>
    private static IReadOnlyList<string> ComputeFastPathNames(string baseName, IReadOnlyList<IMethodSymbol> methods)
    {
        var firstAttempt = methods
            .Select(m => baseName + string.Concat(m.Parameters.Select(p => Capitalize(p.Name))))
            .ToList();

        if (firstAttempt.Distinct().Count() == firstAttempt.Count)
            return firstAttempt;

        // Conflict — fall back to type-based naming
        return methods.Select(m =>
        {
            var typeSuffix = string.Concat(m.Parameters.Select(p =>
                Capitalize(SimpleTypeName(p.Type))));
            return baseName + typeSuffix;
        }).ToList();
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];

    private static string SimpleTypeName(ITypeSymbol type)
    {
        var name = type.Name;
        return type.SpecialType switch
        {
            SpecialType.System_Int32 => "Int",
            SpecialType.System_Int64 => "Long",
            SpecialType.System_String => "String",
            SpecialType.System_Boolean => "Bool",
            SpecialType.System_Double => "Double",
            SpecialType.System_Single => "Float",
            _ => name,
        };
    }
}
