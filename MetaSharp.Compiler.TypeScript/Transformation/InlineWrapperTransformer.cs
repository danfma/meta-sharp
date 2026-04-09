using MetaSharp.Compiler;
using MetaSharp.TypeScript.AST;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaSharp.Transformation;

/// <summary>
/// Transforms a C# struct annotated with <c>[InlineWrapper]</c> into a branded TypeScript
/// type plus a companion namespace.
///
/// Output shape (e.g., for <c>readonly record struct UserId(string Value)</c>):
/// <code>
/// export type UserId = string &amp; { readonly __brand: "UserId" };
/// export namespace UserId {
///     export function create(value: string): UserId { return value as UserId; }
///     // toString() only emitted when the underlying primitive isn't already string
///     // ...plus any user-defined static methods on the struct
/// }
/// </code>
///
/// <see cref="Transform"/> returns <c>false</c> when the type is not actually an inline
/// wrapper (no <c>[InlineWrapper]</c>, not a struct, or its single value member doesn't
/// map to a primitive), letting the caller fall through to the next per-shape transformer.
/// </summary>
public sealed class InlineWrapperTransformer(TypeScriptTransformContext context)
{
    private readonly TypeScriptTransformContext _context = context;

    public bool Transform(INamedTypeSymbol type, List<TsTopLevel> statements)
    {
        if (!SymbolHelper.HasInlineWrapper(type))
            return false;
        if (type.TypeKind != TypeKind.Struct)
            return false;

        var tsTypeName = TypeTransformer.GetTsTypeName(type);
        if (!TypeTransformer.TryGetInlineWrapperPrimitiveType(type, out var primitiveType))
            return false;

        // export type UserId = string & { readonly __brand: "UserId" };
        var brandType = new TsNamedType($"{{ readonly __brand: \"{tsTypeName}\" }}");
        statements.Add(new TsTypeAlias(tsTypeName, new TsIntersectionType([primitiveType, brandType])));

        // Build companion namespace functions
        var functions = new List<TsFunction>();

        // create(value: T): TypeName
        functions.Add(new TsFunction(
            "create",
            [new TsParameter("value", primitiveType)],
            new TsNamedType(tsTypeName),
            [new TsReturnStatement(new TsCastExpression(new TsIdentifier("value"), new TsNamedType(tsTypeName)))],
            Exported: true
        ));

        // toString(value: TypeName): string — only for non-string primitives
        if (primitiveType is not TsStringType)
        {
            functions.Add(new TsFunction(
                "toString",
                [new TsParameter("value", new TsNamedType(tsTypeName))],
                new TsStringType(),
                [new TsReturnStatement(new TsCallExpression(new TsIdentifier("String"), [new TsIdentifier("value")]))],
                Exported: true
            ));
        }

        // Static methods from the struct
        foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
        {
            if (method.MethodKind != MethodKind.Ordinary) continue;
            if (!method.IsStatic) continue;
            if (method.IsImplicitlyDeclared) continue;
            if (method.DeclaredAccessibility != Accessibility.Public) continue;
            if (SymbolHelper.HasIgnore(method)) continue;
            if (TypeScriptNaming.HasEmit(method)) continue;

            var methodSyntax = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
            if (methodSyntax is null) continue;

            var semanticModel = _context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            var exprTransformer = _context.CreateExpressionTransformer(semanticModel);
            var body = exprTransformer.TransformBody(methodSyntax.Body, methodSyntax.ExpressionBody,
                isVoid: method.ReturnsVoid);
            var parameters = method.Parameters
                .Select(p => new TsParameter(TypeScriptNaming.ToCamelCase(p.Name), TypeMapper.Map(p.Type)))
                .ToList();

            // Inline wrapper helpers lower to `export namespace TypeName { export
            // function methodName() { … } }`. Function declarations inside a namespace
            // can NOT use reserved words (`function new() {}` is illegal even though
            // `obj.new` is fine), so this stays on the escaping ToCamelCase variant.
            // The call-site MemberAccessHandler detects [InlineWrapper] receivers and
            // matches the escape so the two halves stay in sync.
            var methodName = SymbolHelper.GetNameOverride(method) ?? TypeScriptNaming.ToCamelCase(method.Name);
            var returnType = TypeMapper.Map(method.ReturnType);
            functions.Add(new TsFunction(methodName, parameters, returnType, body,
                Exported: true, Async: method.IsAsync));
        }

        // export namespace TypeName { ... }
        statements.Add(new TsNamespaceDeclaration(tsTypeName, functions));
        return true;
    }
}
