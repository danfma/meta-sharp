# ADR-0017 — `[NoContainer]` and explicit `InlineMode` on `[Inline]`

**Status:** Accepted
**Date:** 2026-04-30

## Context

ADR-0015 introduced `[Erasable]` to (a) suppress emission of a static
class declaration and (b) flatten the qualifier on every static-member
access at the call site. Once `[Inline]` shipped under the same ADR,
the lowering acquired a second, undocumented job: an `[Inline]` member
inside an `[Erasable]` container β-reduced its body **textually** at
the call site, while an `[Inline]` member outside an `[Erasable]`
container emitted an IIFE wrapper so caller arguments evaluated
exactly once. The cascade was implemented as a chain walk over
`ContainingType` ancestors (`HasErasableInChain`).

Two problems surfaced:

1. **The attribute name implied a single job, but the implementation
   had two.** Authors who put `[Erasable]` on a static class to drop
   the qualifier silently changed the inline-lowering shape of every
   `[Inline]` member declared inside it. Reviewers reading the source
   could not predict which form a call site would emit without
   reading the cascade rule too.
2. **The cascade leaked across nesting boundaries.** A C# 14
   `extension(R r) { … }` block surfaces members through a synthetic
   empty-name nested type. The walk had to step past the synthetic
   type to find the user's `[Erasable]` annotation, which means the
   "Erasable propagates" rule had a special case the user could not
   see in their source.

The DOM-binding catalog in `Metano.TypeScript.DOM` was the only
production user of the cascade, via
`DocumentExtensions.CreateElement<TElement>`. Even there the textual
β-reduction was a deliberate choice (single-use parameter, zero
side-effect risk), not an implicit one.

## Decision

Split the two jobs into two independent, locally-scoped attributes.

1. **Rename `[Erasable]` to `[NoContainer]`.** The attribute now does
   exactly one thing: the static class it decorates emits no
   declaration in the generated output, and every static-member
   access drops the class qualifier. The effect is local to the
   decorated type — it does not propagate to nested types or to
   `[Inline]` members anywhere inside it.

2. **Add `InlineMode` enum to `[Inline]`.** The lowering shape is now
   selected explicitly per member:

   ```csharp
   public enum InlineMode
   {
       Materialize = 0,   // default — IIFE wrap, args eval once
       Substitute  = 1,   // β-reduction, args inline directly
   }
   ```

   `[Inline]` defaults to `InlineMode.Materialize`. Authors who need
   the textual β-reduction (catalog-style helpers, single-use
   wrappers, where the duplication of arguments is intentional)
   write `[Inline(InlineMode.Substitute)]` on the member.

   Class-level `[Inline]` propagation continues to apply (catalog
   classes can still mark every member by attributing the class), and
   the propagation reads the class-level `Mode` so a
   `[NoContainer, Inline(InlineMode.Substitute)]` class still emits
   its members textually without per-member repetition.

`HasErasableInChain` (the cascade walker) is removed from
`IrExpressionExtractor`. The lowering decision now consults
`SymbolHelper.GetInlineMode(method)` exclusively.

## Consequences

- (+) Each attribute names exactly one job. Authors and reviewers can
  predict the lowered shape without tracing a chain walk.
- (+) `[NoContainer]` semantics match `Inherited = false` exactly —
  no cascade across `extension(R r) { … }` synthetic types or any
  other nesting.
- (+) `InlineMode.Materialize` is the safer default (single
  evaluation), so the `[Inline]` member that previously could be
  read either way now defaults to the safer shape unless the author
  opts out.
- (+) Per-member granularity. A class can mix
  `[Inline]` Materialize members and
  `[Inline(InlineMode.Substitute)]` members.
- (−) Breaking change for external users. Every existing `[Erasable]`
  source attribute must rename, and every `[Inline]` that relied on
  the implicit cascade must add `(InlineMode.Substitute)` if the
  textual shape was the intended outcome. Project has no external
  users yet, so the migration is contained.
- (−) `InlineMode.Materialize` adds a closure indirection at every
  call site. JS engines inline trivial IIFEs, so the runtime cost is
  zero in hot paths; the bundle-size cost is a few bytes per call
  site for non-trivial bodies. Users who care explicitly select
  `Substitute`.

## Alternatives considered

- **Keep cascade, add `[Erasable(QualifierOnly = true)]` opt-out.**
  Rejected — the property name signals "exception to a special
  rule," which leaves the rule itself in place. The cleaner fix is
  to remove the rule.
- **Keep cascade, expand `[Erasable]` to apply on methods too
  (member-level Erasable).** Rejected — `[Erasable]` on a method
  means "no declaration emitted, body inlines at call site," which
  is exactly what `[Inline]` already covers. The two would be
  synonyms.
- **Single `[Inline(Textual = true)]` boolean instead of an enum.**
  Rejected — "Textual" is misleading when the inlined value can be a
  number, an object, or any other expression. The enum names the
  lowering shape by behavior, not by output type.

## References

- `src/Metano/Annotations/NoContainerAttribute.cs`
- `src/Metano/Annotations/InlineAttribute.cs`
- `src/Metano.Compiler/SymbolHelper.cs` (`GetInlineMode`)
- `src/Metano.Compiler/Extraction/IrExpressionExtractor.cs`
  (`TryExpandInlineMethod`)
- ADR-0015 — Attribute family for compile-time erasure and inlining
  (this ADR supersedes the cascade rule introduced there)
- Issue #193 — `[Inline]` body materialization
- Commit `358fb37` (rename + InlineMode landing)
