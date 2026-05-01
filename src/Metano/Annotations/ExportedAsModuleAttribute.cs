namespace Metano.Annotations;

/// <summary>
/// <b>Superseded by</b> <see cref="NoContainerAttribute"/>. Existing
/// callers should migrate; this attribute stays fully functional until
/// it is removed in a future release.
/// <para>
/// <c>[NoContainer]</c> produces the same top-level emission and
/// additionally flattens call-site access (<c>ClassName.member</c> →
/// <c>member</c>), closing a latent bug where cross-module references
/// to an <c>[ExportedAsModule]</c> class emitted dangling
/// <c>ClassName.member</c> without a TypeScript-side class declaration.
/// </para>
/// </summary>
[Obsolete("Use [NoContainer] instead. [ExportedAsModule] will be removed in a future release.")]
[AttributeUsage(AttributeTargets.Class)]
public sealed class ExportedAsModuleAttribute : Attribute;
