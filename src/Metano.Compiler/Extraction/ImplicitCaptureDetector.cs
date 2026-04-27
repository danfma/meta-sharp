using Metano.Compiler.IR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metano.Compiler.Extraction;

/// <summary>
/// Detects primary-constructor parameters that are captured implicitly
/// by a class's member bodies. C# 12 primary constructors expose their
/// parameters to every member in the type; when a non-constructor
/// member references one, Roslyn synthesizes a hidden backing field
/// to hold the value. Without the same synthesis on our side the
/// emitted TypeScript drops the parameter from the ctor signature
/// and references an out-of-scope identifier inside member bodies.
/// <para>
/// This detector walks the class's syntax once before member extraction
/// runs, semantically resolves identifier references inside non-ctor
/// members, and produces:
/// </para>
/// <list type="bullet">
///   <item>A <c>param-symbol → field-name</c> map that the
///   <see cref="IrExpressionExtractor"/> consults to rewrite captured
///   identifier references into <c>this._field</c> accesses.</item>
///   <item>A list of synthesized <see cref="IrFieldDeclaration"/>
///   entries to append to the class's member list, one per captured
///   param that lacks an explicit user-written field initializer.</item>
/// </list>
/// <para>
/// Parameters captured by an explicit field initializer
/// (<c>private readonly Foo _foo = foo;</c>) keep going through the
/// existing <c>AnnotateCapturedParams</c> pass so this detector
/// reuses the user's field name and does not synthesize a duplicate.
/// </para>
/// </summary>
internal static class ImplicitCaptureDetector
{
    internal sealed record Result(
        IReadOnlyDictionary<ISymbol, string> ParamFieldMap,
        IReadOnlyList<IrFieldDeclaration> SynthesizedFields
    );

    private static readonly Result Empty = new(
        new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default),
        Array.Empty<IrFieldDeclaration>()
    );

    public static Result Scan(
        INamedTypeSymbol type,
        Compilation? compilation,
        IrTypeOriginResolver? originResolver
    )
    {
        if (compilation is null)
            return Empty;

        var primaryCtor = type.Constructors.FirstOrDefault(IsPrimaryConstructor);
        if (primaryCtor is null || primaryCtor.Parameters.Length == 0)
            return Empty;

        var primaryParams = new Dictionary<ISymbol, IParameterSymbol>(
            SymbolEqualityComparer.Default
        );
        foreach (var param in primaryCtor.Parameters)
            primaryParams[param] = param;

        // Parameters captured by an explicit field initializer get their
        // synthesis suppressed early — the existing AnnotateCapturedParams
        // pass already wires the field/param pair end-to-end. The guard
        // also stops us from misclassifying the initializer's identifier
        // (which lives inside the type's syntax tree) as an implicit
        // capture.
        var explicitlyCapturedParams = CollectExplicitlyCapturedParams(type, primaryParams);

        var primaryCtorSyntaxNodes = CollectPrimaryCtorSyntaxNodes(type);
        var existingFieldNames = type.GetMembers()
            .OfType<IFieldSymbol>()
            .Select(f => f.Name)
            .ToHashSet(StringComparer.Ordinal);
        var captured = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

        foreach (var declaringRef in type.DeclaringSyntaxReferences)
        {
            var typeSyntax = declaringRef.GetSyntax();
            var model = compilation.GetSemanticModel(typeSyntax.SyntaxTree);

            foreach (var identifier in typeSyntax.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (IsInsidePrimaryCtor(identifier, primaryCtorSyntaxNodes))
                    continue;
                var resolved = model.GetSymbolInfo(identifier).Symbol;
                if (
                    resolved is IParameterSymbol paramSymbol
                    && primaryParams.ContainsKey(paramSymbol)
                    && !explicitlyCapturedParams.Contains(paramSymbol)
                )
                {
                    captured.Add(paramSymbol);
                }
            }
        }

        if (captured.Count == 0)
            return Empty;

        var paramFieldMap = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);
        var synthesized = new List<IrFieldDeclaration>(captured.Count);
        foreach (var symbol in captured)
        {
            if (symbol is not IParameterSymbol paramSymbol)
                continue;

            var fieldName = SynthesizeFieldName(paramSymbol.Name);

            // Hand-written field with the same name preempts the
            // synthesized backing field — silently skip the
            // synthesis. The bare param reference inside member
            // bodies stays unrewritten and the consumer's TS
            // compiler surfaces the missing identifier. Surfacing a
            // dedicated diagnostic would require threading
            // <see cref="MetanoDiagnostic"/> up from the extractor;
            // tracked as a follow-up once the extraction pipeline
            // gains a diagnostic sink.
            if (existingFieldNames.Contains(fieldName))
                continue;

            existingFieldNames.Add(fieldName);
            paramFieldMap[paramSymbol] = fieldName;
            synthesized.Add(
                new IrFieldDeclaration(
                    Name: fieldName,
                    Visibility: IrVisibility.Private,
                    IsStatic: false,
                    Type: IrTypeRefMapper.Map(paramSymbol.Type, originResolver),
                    IsReadonly: true,
                    Initializer: null,
                    IsCapturedByCtor: true
                )
            );
        }

        return new Result(paramFieldMap, synthesized);
    }

    /// <summary>
    /// True for the synthesized C# 12+ primary constructor — the
    /// instance constructor whose declaring syntax is the type
    /// declaration itself rather than a separate
    /// <c>ConstructorDeclarationSyntax</c>.
    /// </summary>
    private static bool IsPrimaryConstructor(IMethodSymbol ctor)
    {
        if (ctor.MethodKind != MethodKind.Constructor || ctor.IsStatic)
            return false;

        var syntax = ctor.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        return syntax
            is ClassDeclarationSyntax
                or StructDeclarationSyntax
                or RecordDeclarationSyntax;
    }

    /// <summary>
    /// Collects parameters already captured by an explicit field
    /// initializer of the form <c>= paramName</c>, so the detector
    /// does not synthesize a second backing field for them. The
    /// existing <c>AnnotateCapturedParams</c> pass owns those
    /// captures end-to-end.
    /// </summary>
    private static HashSet<IParameterSymbol> CollectExplicitlyCapturedParams(
        INamedTypeSymbol type,
        Dictionary<ISymbol, IParameterSymbol> primaryParams
    )
    {
        var result = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
        foreach (var field in type.GetMembers().OfType<IFieldSymbol>())
        {
            var declaration =
                field.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()
                as VariableDeclaratorSyntax;
            if (declaration?.Initializer?.Value is not IdentifierNameSyntax initializerId)
                continue;

            foreach (var param in primaryParams.Values)
            {
                if (
                    string.Equals(
                        param.Name,
                        initializerId.Identifier.ValueText,
                        StringComparison.Ordinal
                    )
                )
                    result.Add(param);
            }
        }
        return result;
    }

    /// <summary>
    /// Returns the syntax nodes that delimit the primary
    /// constructor's body — the type's parameter list and any base
    /// initializer arguments, the only places where primary-ctor
    /// parameters are in scope as plain locals. Identifier
    /// references inside these nodes do not need rewriting.
    /// </summary>
    private static IReadOnlyList<SyntaxNode> CollectPrimaryCtorSyntaxNodes(INamedTypeSymbol type)
    {
        var nodes = new List<SyntaxNode>();
        foreach (var declaringRef in type.DeclaringSyntaxReferences)
        {
            var syntax = declaringRef.GetSyntax();
            switch (syntax)
            {
                case ClassDeclarationSyntax cls:
                    if (cls.ParameterList is not null)
                        nodes.Add(cls.ParameterList);
                    AddPrimaryBaseArguments(cls.BaseList, nodes);
                    break;
                case StructDeclarationSyntax st:
                    if (st.ParameterList is not null)
                        nodes.Add(st.ParameterList);
                    AddPrimaryBaseArguments(st.BaseList, nodes);
                    break;
                case RecordDeclarationSyntax rec:
                    if (rec.ParameterList is not null)
                        nodes.Add(rec.ParameterList);
                    AddPrimaryBaseArguments(rec.BaseList, nodes);
                    break;
            }
        }
        return nodes;
    }

    private static void AddPrimaryBaseArguments(BaseListSyntax? baseList, List<SyntaxNode> nodes)
    {
        var primaryBase = baseList
            ?.Types.OfType<PrimaryConstructorBaseTypeSyntax>()
            .FirstOrDefault();
        if (primaryBase?.ArgumentList is not null)
            nodes.Add(primaryBase.ArgumentList);
    }

    private static bool IsInsidePrimaryCtor(
        SyntaxNode identifier,
        IReadOnlyList<SyntaxNode> primaryCtorScopes
    )
    {
        foreach (var scope in primaryCtorScopes)
        {
            if (scope.Span.Contains(identifier.Span))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Picks the synthesized field name from a primary-ctor param's
    /// source name. The convention matches the rest of the code base:
    /// camelCase parameter prefixed with an underscore
    /// (<c>view</c> → <c>_view</c>, <c>onClick</c> → <c>_onClick</c>).
    /// Reserved JS keywords already get their reserved-word treatment
    /// downstream when the backend renders the field, so the leading
    /// underscore is enough here.
    /// </summary>
    private static string SynthesizeFieldName(string paramName)
    {
        if (string.IsNullOrEmpty(paramName))
            return "_value";
        var first = char.ToLowerInvariant(paramName[0]);
        return paramName.Length == 1 ? "_" + first : "_" + first + paramName[1..];
    }
}
