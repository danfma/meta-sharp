/// Base class for all Metano-generated value types (records, branded types,
/// and any type the compiler emits with synthesized value-based equality).
///
/// Subclasses are expected to override [operator ==] and [hashCode] — the
/// Metano compiler generates these implementations from the C# source. Having
/// a shared base lets the runtime and generated code agree on a single
/// "is this a Metano value object?" check (`obj is MetanoObject`) without
/// relying on duck typing.
abstract class MetanoObject {
  const MetanoObject();
}
