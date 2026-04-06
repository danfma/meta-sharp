namespace MetaSharp;

/// <summary>
/// Maps a BCL type to a JavaScript package type.
/// Applied at assembly level to configure type mappings for transpilation.
/// </summary>
/// <example>
/// [assembly: ExportFromBcl(typeof(decimal), FromPackage = "decimal.js", ExportedName = "Decimal")]
/// </example>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ExportFromBclAttribute(Type type) : Attribute
{
    public Type Type { get; } = type;
    public string FromPackage { get; set; } = "";
    public string ExportedName { get; set; } = "";
}
