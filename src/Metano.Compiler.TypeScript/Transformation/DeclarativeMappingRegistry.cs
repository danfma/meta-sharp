using Microsoft.CodeAnalysis;

namespace Metano.Transformation;

/// <summary>
/// One declarative mapping entry from <c>[MapMethod]</c> or <c>[MapProperty]</c>.
/// Either <see cref="JsName"/> (simple rename) or <see cref="JsTemplate"/> is set, never
/// both. The reader enforces the mutual exclusivity at registration time.
///
/// <see cref="WhenArg0StringEquals"/> is an optional literal-argument filter: when set,
/// the entry only matches a call site whose first argument is a TS string literal with
/// that exact value. Used for literal-aware lowering like <c>Guid.ToString("N")</c>.
///
/// <see cref="WrapReceiver"/> is an optional source-receiver wrapping spec: when set,
/// the call site's receiver is rewritten to <c>WrapReceiver(source)</c> before the
/// rename/template is applied, unless the receiver is already a chained call from the
/// same wrapper (LINQ-style). See <see cref="MapMethodAttribute.WrapReceiver"/>.
///
/// <see cref="RuntimeImports"/> is an optional runtime-helper identifier the lowered
/// call site needs imported from <c>metano-runtime</c>. Forwarded to the
/// generated <see cref="TsTemplate"/> so the import collector can pick it up — the
/// template body is otherwise opaque text from the walker's perspective.
/// </summary>
public sealed record DeclarativeMappingEntry(
    string? JsName,
    string? JsTemplate,
    string? WhenArg0StringEquals = null,
    string? WrapReceiver = null,
    string? RuntimeImports = null
)
{
    public bool HasTemplate => JsTemplate is not null;

    public bool HasArgFilter => WhenArg0StringEquals is not null;

    public bool HasWrapReceiver => WrapReceiver is not null;
}

/// <summary>
/// Index of declarative BCL mappings collected from all <c>[assembly: MapMethod]</c> and
/// <c>[assembly: MapProperty]</c> attributes visible to a Roslyn <see cref="Compilation"/>
/// (the current assembly + every referenced assembly).
///
/// Built once during <see cref="TypeTransformer.TransformAll"/> setup, stored on the
/// <see cref="TypeScriptTransformContext"/>, and consulted by <see cref="BclMapper"/>
/// before falling back to its hardcoded lowering rules.
///
/// Lookup is keyed by <c>(declaringType.OriginalDefinition, memberName)</c> so that an
/// entry registered as <c>typeof(List&lt;&gt;)</c> matches every closed instantiation like
/// <c>List&lt;int&gt;</c> or <c>List&lt;Money&gt;</c>.
/// </summary>
public sealed class DeclarativeMappingRegistry
{
    private readonly Dictionary<
        (INamedTypeSymbol Type, string Name),
        List<DeclarativeMappingEntry>
    > _methods;
    private readonly Dictionary<
        (INamedTypeSymbol Type, string Name),
        DeclarativeMappingEntry
    > _properties;
    private readonly Dictionary<string, HashSet<string>> _chainMethodsByWrapper;

    private DeclarativeMappingRegistry(
        Dictionary<(INamedTypeSymbol, string), List<DeclarativeMappingEntry>> methods,
        Dictionary<(INamedTypeSymbol, string), DeclarativeMappingEntry> properties,
        Dictionary<string, HashSet<string>> chainMethodsByWrapper
    )
    {
        _methods = methods;
        _properties = properties;
        _chainMethodsByWrapper = chainMethodsByWrapper;
    }

    /// <summary>
    /// Empty registry — used when there are no declarative mappings to honor (e.g., the
    /// compilation does not reference any assembly that defines them).
    /// </summary>
    public static DeclarativeMappingRegistry Empty { get; } =
        new(
            new Dictionary<(INamedTypeSymbol, string), List<DeclarativeMappingEntry>>(
                SymbolNameKeyComparer.Instance
            ),
            new Dictionary<(INamedTypeSymbol, string), DeclarativeMappingEntry>(
                SymbolNameKeyComparer.Instance
            ),
            []
        );

    public int MethodCount => _methods.Values.Sum(list => list.Count);
    public int PropertyCount => _properties.Count;

    /// <summary>
    /// Returns the set of JS method names registered for a given <c>WrapReceiver</c>
    /// value. Used by <see cref="BclMapper"/> to recognize "already wrapped" sources
    /// when applying a mapping with a wrapping spec — if the receiver is a call whose
    /// callee property matches one of these names, no re-wrapping is needed.
    /// </summary>
    public IReadOnlySet<string> GetChainMethodNames(string wrapReceiver) =>
        _chainMethodsByWrapper.TryGetValue(wrapReceiver, out var set)
            ? set
            : (IReadOnlySet<string>)new HashSet<string>();

    /// <summary>
    /// Returns all declarative method mapping entries registered for the given containing
    /// type + method name, in declaration order. Multiple entries are allowed when an
    /// entry uses the optional <see cref="DeclarativeMappingEntry.WhenArg0StringEquals"/>
    /// filter to discriminate between literal arg shapes (e.g.,
    /// <c>Guid.ToString("N")</c> vs <c>Guid.ToString()</c>). Callers walk the list and
    /// pick the first entry whose filter matches their call site.
    ///
    /// The containing type is normalized to its
    /// <see cref="INamedTypeSymbol.OriginalDefinition"/> so closed generics resolve to
    /// the open-generic registration.
    /// </summary>
    public bool TryGetMethods(
        INamedTypeSymbol containingType,
        string methodName,
        out IReadOnlyList<DeclarativeMappingEntry> entries
    )
    {
        if (_methods.TryGetValue((containingType.OriginalDefinition, methodName), out var list))
        {
            entries = list;
            return true;
        }

        entries = [];
        return false;
    }

    /// <summary>
    /// Looks up a declarative property mapping by the containing type and the C# property name.
    /// </summary>
    public bool TryGetProperty(
        INamedTypeSymbol containingType,
        string propertyName,
        out DeclarativeMappingEntry entry
    ) => _properties.TryGetValue((containingType.OriginalDefinition, propertyName), out entry!);

    /// <summary>
    /// Builds a registry by walking the compilation's own assembly and every referenced
    /// assembly, collecting their assembly-level <c>[MapMethod]</c> and <c>[MapProperty]</c>
    /// attributes. Mappings whose <c>DeclaringType</c> can't be resolved against the current
    /// compilation (e.g., a referenced assembly mentions a type the consumer doesn't ship)
    /// are silently skipped.
    /// </summary>
    public static DeclarativeMappingRegistry BuildFromCompilation(Compilation compilation)
    {
        var methods = new Dictionary<(INamedTypeSymbol, string), List<DeclarativeMappingEntry>>(
            SymbolNameKeyComparer.Instance
        );
        var properties = new Dictionary<(INamedTypeSymbol, string), DeclarativeMappingEntry>(
            SymbolNameKeyComparer.Instance
        );

        // Walk the current assembly + every referenced assembly's attributes
        var assemblies = new List<IAssemblySymbol> { compilation.Assembly };
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
                assemblies.Add(assembly);
        }

        foreach (var assembly in assemblies)
        {
            foreach (var attr in assembly.GetAttributes())
            {
                var attrName = attr.AttributeClass?.Name;
                if (attrName is "MapMethodAttribute")
                    TryRegisterMethod(attr, methods);
                else if (attrName is "MapPropertyAttribute")
                    TryRegisterProperty(attr, properties);
            }
        }

        // Build the per-wrapper chain method index used by BclMapper to detect
        // already-wrapped receivers. Every entry that uses WrapReceiver contributes its
        // JsName (or the inferred wrap-method root) to its wrapper's set.
        var chainMethodsByWrapper = new Dictionary<string, HashSet<string>>();
        foreach (var entries in methods.Values)
        {
            foreach (var entry in entries)
            {
                if (entry.WrapReceiver is null)
                    continue;
                if (entry.JsName is null)
                    continue; // template entries can't be detected by name

                if (!chainMethodsByWrapper.TryGetValue(entry.WrapReceiver, out var set))
                {
                    set = [];
                    chainMethodsByWrapper[entry.WrapReceiver] = set;
                }
                set.Add(entry.JsName);
            }
        }

        return new DeclarativeMappingRegistry(methods, properties, chainMethodsByWrapper);
    }

    /// <summary>
    /// Reads one [MapMethod] attribute and appends it to the per-key list in the target
    /// dictionary. Multiple entries per key are allowed when filters like
    /// <c>WhenArg0StringEquals</c> are used to discriminate between literal arg shapes.
    /// Entries are appended in walk order (current assembly first, then references), so
    /// the consumer's source order within an assembly is preserved.
    /// </summary>
    private static void TryRegisterMethod(
        AttributeData attr,
        Dictionary<(INamedTypeSymbol, string), List<DeclarativeMappingEntry>> target
    )
    {
        var entry = ReadEntry(attr, "JsMethod");
        if (entry is null)
            return;

        var declaringType = (INamedTypeSymbol)attr.ConstructorArguments[0].Value!;
        var memberName = (string)attr.ConstructorArguments[1].Value!;
        var key = (declaringType.OriginalDefinition, memberName);

        if (!target.TryGetValue(key, out var list))
        {
            list = [];
            target[key] = list;
        }
        list.Add(entry);
    }

    /// <summary>
    /// Reads one [MapProperty] attribute and stores it in the target dictionary. Property
    /// mappings don't use literal-argument filters (properties have no arguments), so
    /// the storage stays single-entry-per-key with last-write-wins semantics.
    /// </summary>
    private static void TryRegisterProperty(
        AttributeData attr,
        Dictionary<(INamedTypeSymbol, string), DeclarativeMappingEntry> target
    )
    {
        var entry = ReadEntry(attr, "JsProperty");
        if (entry is null)
            return;

        var declaringType = (INamedTypeSymbol)attr.ConstructorArguments[0].Value!;
        var memberName = (string)attr.ConstructorArguments[1].Value!;
        var key = (declaringType.OriginalDefinition, memberName);

        target[key] = entry;
    }

    /// <summary>
    /// Reads the body of a [MapMethod]/[MapProperty] AttributeData into a
    /// <see cref="DeclarativeMappingEntry"/>, returning null when the constructor args
    /// are missing or when neither <c>JsMethod</c>/<c>JsProperty</c> nor <c>JsTemplate</c>
    /// is provided.
    /// </summary>
    private static DeclarativeMappingEntry? ReadEntry(AttributeData attr, string renameNamedArg)
    {
        if (attr.ConstructorArguments.Length < 2)
            return null;
        if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol)
            return null;
        if (attr.ConstructorArguments[1].Value is not string)
            return null;

        string? jsName = null;
        string? jsTemplate = null;
        string? whenArg0StringEquals = null;
        string? wrapReceiver = null;
        string? runtimeImports = null;
        foreach (var named in attr.NamedArguments)
        {
            switch (named.Key)
            {
                case var k when k == renameNamedArg:
                    jsName = named.Value.Value as string;
                    break;
                case "JsTemplate":
                    jsTemplate = named.Value.Value as string;
                    break;
                case "WhenArg0StringEquals":
                    whenArg0StringEquals = named.Value.Value as string;
                    break;
                case "WrapReceiver":
                    wrapReceiver = named.Value.Value as string;
                    break;
                case "RuntimeImports":
                    runtimeImports = named.Value.Value as string;
                    break;
            }
        }

        if (jsName is null && jsTemplate is null)
            return null;

        return new DeclarativeMappingEntry(
            jsName,
            jsTemplate,
            whenArg0StringEquals,
            wrapReceiver,
            runtimeImports
        );
    }

    /// <summary>
    /// Equality comparer for the (declaringType, memberName) lookup key. Uses
    /// <see cref="SymbolEqualityComparer.Default"/> for the symbol part so that two
    /// references to the same generic definition (e.g., from different syntax trees)
    /// hash and compare equal.
    /// </summary>
    private sealed class SymbolNameKeyComparer
        : IEqualityComparer<(INamedTypeSymbol Type, string Name)>
    {
        public static readonly SymbolNameKeyComparer Instance = new();

        public bool Equals(
            (INamedTypeSymbol Type, string Name) x,
            (INamedTypeSymbol Type, string Name) y
        ) => SymbolEqualityComparer.Default.Equals(x.Type, y.Type) && x.Name == y.Name;

        public int GetHashCode((INamedTypeSymbol Type, string Name) obj) =>
            unchecked(
                SymbolEqualityComparer.Default.GetHashCode(obj.Type) * 397 ^ obj.Name.GetHashCode()
            );
    }
}
