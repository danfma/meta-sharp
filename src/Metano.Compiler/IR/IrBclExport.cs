namespace Metano.Compiler.IR;

/// <summary>
/// A <c>[ExportFromBcl]</c> entry declared on a referenced assembly. Lets a
/// BCL type (<c>decimal</c>, …) map to a concrete library package in every
/// target that consumes the assembly — e.g., decimal → <c>Decimal</c> from
/// <c>decimal.js</c> on the TypeScript target.
/// </summary>
/// <param name="ExportedName">Identifier emitted in the target source.</param>
/// <param name="FromPackage">Package name the identifier is imported
/// from.</param>
/// <param name="Version">Semver specifier the backend merges into its
/// package manifest.</param>
public sealed record IrBclExport(string ExportedName, string FromPackage, string Version);
