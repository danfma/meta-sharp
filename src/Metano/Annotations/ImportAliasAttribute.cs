namespace Metano.Annotations;

/// <summary>
/// Per-file import-alias metadata for the TypeScript target. Declared on a
/// C# 11 <c>file</c>-scoped class carrier (typically empty) at the top of
/// the source file the alias should apply to. Lets a user pin the local
/// name an imported type binds to inside the emitted module without
/// polluting the C# code with <c>using X = Y;</c> directives, and supports
/// per-target divergence (different alias for TypeScript vs Dart) and
/// bulk suffixing.
///
/// <para>
/// Two construction shapes:
/// </para>
///
/// <para>
/// <strong>Single</strong> — pin one type to one alias:
/// </para>
///
/// <code>
/// [ImportAlias(typeof(Column), "ColumnWidget", Target = TargetLanguage.TypeScript)]
/// [ImportAlias(typeof(Column), "ColumnW",      Target = TargetLanguage.Dart)]
/// file class TsModule;
/// </code>
///
/// <para>
/// <strong>Bulk</strong> — apply a common suffix to every type in <see cref="Types"/>:
/// </para>
///
/// <code>
/// [ImportAlias(Suffix = "Widget", Types = [typeof(Row), typeof(Text), typeof(Heading)])]
/// file class TsModule;
/// </code>
///
/// <para>
/// Precedence: <c>[ImportAlias]</c> &gt; <c>using X = Y;</c> &gt; auto-aliased
/// fallback &gt; canonical name. The carrier itself is not transpiled.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ImportAliasAttribute : Attribute
{
    /// <summary>Single-type form — pin one type to one alias.</summary>
    public ImportAliasAttribute(Type type, string alias)
    {
        Type = type;
        Alias = alias;
    }

    /// <summary>Single-type, target-scoped form.</summary>
    public ImportAliasAttribute(TargetLanguage target, Type type, string alias)
    {
        Target = target;
        Type = type;
        Alias = alias;
    }

    /// <summary>Bulk form — set <see cref="Suffix"/> + <see cref="Types"/> via initializer.</summary>
    public ImportAliasAttribute() { }

    public Type? Type { get; }
    public string? Alias { get; }
    public string? Suffix { get; init; }
    public Type[]? Types { get; init; }

    /// <summary>Optional target filter. <c>null</c> applies to every backend.</summary>
    public TargetLanguage? Target { get; init; }
}
