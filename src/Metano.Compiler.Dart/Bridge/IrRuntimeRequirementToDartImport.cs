using Metano.Compiler.IR;
using Metano.Dart.AST;

namespace Metano.Dart.Bridge;

/// <summary>
/// Maps target-agnostic <see cref="IrRuntimeRequirement"/> facts into concrete
/// Dart <see cref="DartImport"/> lines. The translation is Dart-specific (choice
/// of package, <c>show</c> clauses) and lives in the Dart target — the IR itself
/// only says "this module needs the HashCode helper"; each backend decides what
/// that means in its module system.
/// </summary>
public static class IrRuntimeRequirementToDartImport
{
    private const string MetanoRuntimePath = "package:metano_runtime/metano_runtime.dart";

    public static IReadOnlyList<DartImport> Convert(
        IReadOnlySet<IrRuntimeRequirement> requirements
    )
    {
        if (requirements.Count == 0)
            return [];

        // Group helpers that share the same target module so each import line
        // collapses to a single `import '...' show A, B;`. Dart's `show` clause
        // keeps the import surface explicit and lets the analyzer flag stale
        // requirements during refactors.
        var byPath = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);
        foreach (var req in requirements)
        {
            var mapping = Map(req);
            if (mapping is null)
                continue;

            if (!byPath.TryGetValue(mapping.Value.ImportPath, out var names))
            {
                names = new SortedSet<string>(StringComparer.Ordinal);
                byPath[mapping.Value.ImportPath] = names;
            }
            names.Add(mapping.Value.Symbol);
        }

        return byPath
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => new DartImport(kvp.Key, ShowNames: kvp.Value.ToArray()))
            .ToList();
    }

    /// <summary>
    /// Maps a semantic runtime helper to its concrete Dart import. Returning
    /// <c>null</c> means "no import needed" — either the helper is satisfied by
    /// a Dart built-in (e.g., <c>DateTime</c>, <c>Set&lt;T&gt;</c>, the <c>is</c>
    /// operator) or the helper isn't yet ported to <c>metano_runtime</c>.
    /// </summary>
    private static (string ImportPath, string Symbol)? Map(IrRuntimeRequirement req) =>
        req.HelperName switch
        {
            "HashCode" => (MetanoRuntimePath, "HashCode"),
            // Built-in or deferred:
            //   Temporal     → Dart `DateTime` (built-in)
            //   HashSet      → Dart `Set<T>`   (built-in)
            //   isInt32, isString, …  → Dart `is` operator (built-in)
            //   UUID, Grouping, Enumerable, delegateAdd/Remove → not yet ported
            _ => null,
        };
}
