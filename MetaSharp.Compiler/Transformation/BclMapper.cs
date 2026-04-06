using MetaSharp.TypeScript;
using MetaSharp.TypeScript.AST;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaSharp.Transformation;

/// <summary>
/// Maps known BCL methods and properties to JavaScript equivalents.
/// </summary>
public static class BclMapper
{
    /// <summary>
    /// Try to map a member access (property or method without invocation).
    /// </summary>
    public static TsExpression? TryMap(
        ISymbol symbol,
        MemberAccessExpressionSyntax member,
        ExpressionTransformer transformer
    )
    {
        var containing = symbol.ContainingType?.ToDisplayString();

        var obj = transformer.TransformExpression(member.Expression);

        // string instance properties
        if (containing == "string" && symbol.Name == "Length")
            return new TsPropertyAccess(obj, "length");

        // List<T>.Count, ICollection<T>.Count, IReadOnlyCollection<T>.Count → .length
        if (symbol.Name == "Count" && IsCollectionType(containing))
            return new TsPropertyAccess(obj, "length");

        // Dictionary<K,V>.Count, HashSet<T>.Count → .size
        if (symbol.Name == "Count" && IsMapOrSetType(containing))
            return new TsPropertyAccess(obj, "size");

        return null;
    }

    /// <summary>
    /// Try to map a method invocation to a JS equivalent.
    /// </summary>
    public static TsExpression? TryMapMethod(
        IMethodSymbol method,
        InvocationExpressionSyntax invocation,
        ExpressionTransformer transformer
    )
    {
        var containing = method.ContainingType?.ToDisplayString();
        var name = method.Name;

        var args = invocation
            .ArgumentList.Arguments.Select(a => transformer.TransformExpression(a.Expression))
            .ToList();

        // System.Math static methods
        if (containing == "System.Math")
        {
            var jsMethod = name switch
            {
                "Round" => "round",
                "Floor" => "floor",
                "Ceiling" => "ceil",
                "Ceil" => "ceil",
                "Abs" => "abs",
                "Min" => "min",
                "Max" => "max",
                "Sqrt" => "sqrt",
                "Pow" => "pow",
                _ => null,
            };

            if (jsMethod is not null)
                return new TsCallExpression(
                    new TsPropertyAccess(new TsIdentifier("Math"), jsMethod),
                    args
                );
        }

        // string instance methods
        if (
            containing == "string"
            && invocation.Expression is MemberAccessExpressionSyntax memberAccess
        )
        {
            var obj = transformer.TransformExpression(memberAccess.Expression);
            var jsMethod = name switch
            {
                "ToUpper" or "ToUpperInvariant" => "toUpperCase",
                "ToLower" or "ToLowerInvariant" => "toLowerCase",
                "Contains" => "includes",
                "StartsWith" => "startsWith",
                "EndsWith" => "endsWith",
                "Trim" => "trim",
                "TrimStart" => "trimStart",
                "TrimEnd" => "trimEnd",
                "Replace" => "replace",
                "Substring" => "substring",
                "IndexOf" => "indexOf",
                _ => null,
            };

            if (jsMethod is not null)
                return new TsCallExpression(new TsPropertyAccess(obj, jsMethod), args);
        }

        // List<T> / ICollection<T> instance methods
        if (IsCollectionType(containing) && invocation.Expression is MemberAccessExpressionSyntax listAccess)
        {
            var obj = transformer.TransformExpression(listAccess.Expression);
            var jsMethod = name switch
            {
                "Add" => "push",
                "Contains" => "includes",
                "IndexOf" => "indexOf",
                "Remove" => null, // complex — needs splice pattern, skip for now
                "Clear" => null,
                "Insert" => "splice", // approximate
                "Reverse" => "reverse",
                "Sort" => "sort",
                "ToArray" => "slice", // creates a copy
                _ => null,
            };

            if (jsMethod is not null)
                return new TsCallExpression(new TsPropertyAccess(obj, jsMethod), args);

            // Clear() → .length = 0
            if (name == "Clear")
                return new TsBinaryExpression(
                    new TsPropertyAccess(obj, "length"),
                    "=",
                    new TsLiteral("0"));
        }

        // LINQ extension methods → lazy Enumerable chain via @meta-sharp/runtime
        if (IsLinqExtensionMethod(containing) && invocation.Expression is MemberAccessExpressionSyntax linqAccess)
        {
            var source = transformer.TransformExpression(linqAccess.Expression);

            // Only wrap with Enumerable.from() if source is not already a LINQ chain
            var wrapped = IsAlreadyLinqChain(source)
                ? source
                : new TsCallExpression(
                    new TsPropertyAccess(new TsIdentifier("Enumerable"), "from"),
                    [source]);

            return name switch
            {
                // Composition (lazy — returns EnumerableBase)
                "Where"             => LinqCall(wrapped, "where", args),
                "Select"            => LinqCall(wrapped, "select", args),
                "SelectMany"        => LinqCall(wrapped, "selectMany", args),
                "OrderBy"           => LinqCall(wrapped, "orderBy", args),
                "OrderByDescending" => LinqCall(wrapped, "orderByDescending", args),
                "Take"              => LinqCall(wrapped, "take", args),
                "Skip"              => LinqCall(wrapped, "skip", args),
                "Distinct"          => LinqCall(wrapped, "distinct", []),
                "GroupBy"           => LinqCall(wrapped, "groupBy", args),
                "Concat"            => LinqCall(wrapped, "concat", args),

                // Terminal (materializes)
                "ToList" or "ToArray" => LinqCall(wrapped, "toArray", []),
                "First"             => LinqCall(wrapped, "first", args),
                "FirstOrDefault"    => LinqCall(wrapped, "firstOrDefault", args),
                "Last"              => LinqCall(wrapped, "last", args),
                "LastOrDefault"     => LinqCall(wrapped, "lastOrDefault", args),
                "Single"            => LinqCall(wrapped, "single", args),
                "Any"               => LinqCall(wrapped, "any", args),
                "All"               => LinqCall(wrapped, "all", args),
                "Count"             => LinqCall(wrapped, "count", args),
                "Sum"               => LinqCall(wrapped, "sum", args),
                "Min"               => LinqCall(wrapped, "min", args),
                "Max"               => LinqCall(wrapped, "max", args),
                "Contains"          => LinqCall(wrapped, "contains", args),
                "Aggregate"         => LinqCall(wrapped, "aggregate", args),

                _ => null,
            };
        }

        // Dictionary<K,V> instance methods
        if (IsMapOrSetType(containing) && invocation.Expression is MemberAccessExpressionSyntax dictAccess)
        {
            var obj = transformer.TransformExpression(dictAccess.Expression);
            return name switch
            {
                "ContainsKey" => new TsCallExpression(new TsPropertyAccess(obj, "has"), args),
                "TryGetValue" => null, // complex pattern, skip for now
                "Add" when args.Count == 2 => new TsCallExpression(new TsPropertyAccess(obj, "set"), args),
                "Add" when args.Count == 1 => new TsCallExpression(new TsPropertyAccess(obj, "add"), args),
                "Remove" => new TsCallExpression(new TsPropertyAccess(obj, "delete"), args),
                "Clear" => new TsCallExpression(new TsPropertyAccess(obj, "clear"), []),
                "Contains" => new TsCallExpression(new TsPropertyAccess(obj, "has"), args),
                _ => null,
            };
        }

        // Console.WriteLine → console.log
        if (containing == "System.Console" && name == "WriteLine")
            return new TsCallExpression(
                new TsPropertyAccess(new TsIdentifier("console"), "log"),
                args
            );

        return null;
    }

    /// <summary>
    /// Creates a comparator arrow function for sorting: (a, b) => keySelector(a) - keySelector(b)
    /// </summary>

    // ─── Type classification helpers ────────────────────────

    private static bool IsCollectionType(string? fullName)
    {
        if (fullName is null) return false;
        return fullName.StartsWith("System.Collections.Generic.List")
            || fullName.StartsWith("System.Collections.Generic.IList")
            || fullName.StartsWith("System.Collections.Generic.ICollection")
            || fullName.StartsWith("System.Collections.Generic.IReadOnlyList")
            || fullName.StartsWith("System.Collections.Generic.IReadOnlyCollection")
            || fullName.StartsWith("System.Collections.Immutable.ImmutableList")
            || fullName.StartsWith("System.Collections.Immutable.ImmutableArray");
    }

    private static bool IsMapOrSetType(string? fullName)
    {
        if (fullName is null) return false;
        return fullName.StartsWith("System.Collections.Generic.Dictionary")
            || fullName.StartsWith("System.Collections.Generic.IDictionary")
            || fullName.StartsWith("System.Collections.Generic.IReadOnlyDictionary")
            || fullName.StartsWith("System.Collections.Generic.HashSet")
            || fullName.StartsWith("System.Collections.Generic.ISet")
            || fullName.StartsWith("System.Collections.Generic.SortedSet");
    }

    private static bool IsLinqExtensionMethod(string? fullName)
    {
        if (fullName is null) return false;
        return fullName == "System.Linq.Enumerable"
            || fullName.StartsWith("System.Linq.IOrderedEnumerable");
    }

    private static TsCallExpression LinqCall(TsExpression source, string method, List<TsExpression> args) =>
        new(new TsPropertyAccess(source, method), args);

    /// <summary>
    /// Detects if an expression is already the result of a LINQ chain (Enumerable.from() or .where(), etc.)
    /// to avoid double-wrapping.
    /// </summary>
    private static bool IsAlreadyLinqChain(TsExpression expr) => expr switch
    {
        TsCallExpression { Callee: TsPropertyAccess { Object: TsIdentifier { Name: "Enumerable" } } } => true,
        TsCallExpression { Callee: TsPropertyAccess { Property: var p } } when IsLinqMethodName(p) => true,
        _ => false,
    };

    private static bool IsLinqMethodName(string name) => name is
        "where" or "select" or "selectMany" or "orderBy" or "orderByDescending"
        or "take" or "skip" or "distinct" or "groupBy" or "concat"
        or "toArray" or "first" or "firstOrDefault" or "last" or "lastOrDefault"
        or "single" or "any" or "all" or "count" or "sum" or "min" or "max"
        or "contains" or "aggregate";
}
