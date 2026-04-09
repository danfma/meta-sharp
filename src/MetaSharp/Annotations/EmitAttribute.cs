namespace MetaSharp.Annotations;

/// <summary>
/// Emits raw JavaScript at the call site. Use $0, $1, etc. for argument placeholders.
/// </summary>
/// <example>
/// [Emit("$0.toFixed($1)")]
/// public static extern string ToFixed(decimal value, int digits);
/// </example>
[AttributeUsage(AttributeTargets.Method)]
public sealed class EmitAttribute(string expression) : Attribute
{
    public string Expression { get; } = expression;
}
