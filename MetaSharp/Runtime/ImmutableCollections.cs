// Declarative BCL → JavaScript mappings for the System.Collections.Immutable family.
//
// ImmutableList<T> and ImmutableArray<T> both lower to plain JS arrays at the type level
// (handled by TypeMapper.IsCollectionLike). The non-mutating Add / Remove / Insert /
// Clear methods become spread expressions that return a NEW array, mirroring the
// immutable contract — `var newList = old.Add(x);` lowers to
// `const newList = [...old, x];` and the original array is left untouched.
//
// Two flavors of lowering live here:
//
// - **Inline templates** for the trivial single-spread / literal forms (Add, AddRange,
//   Clear, Empty). These read clearly at the call site and need no helper.
// - **Runtime helpers** (immutableInsert / immutableRemoveAt / immutableRemove) for the
//   multi-step algorithms. The helpers live in @meta-sharp/runtime/system/collections/
//   list-helpers.ts and are pulled in via the RuntimeImports schema property. Putting
//   the body in JS instead of in a captured-receiver IIFE keeps the generated code
//   readable, gives the helper a name in stack traces, and lets the runtime own the
//   implementation in a single place.

using System.Collections.Immutable;
using MetaSharp.Annotations;

// ─── ImmutableList<T> ───────────────────────────────────────

// list.Add(item) → [...list, item]
[assembly: MapMethod(typeof(ImmutableList<>), nameof(ImmutableList<int>.Add),
    JsTemplate = "[...$this, $0]")]

// list.AddRange(other) → [...list, ...other]
[assembly: MapMethod(typeof(ImmutableList<>), nameof(ImmutableList<int>.AddRange),
    JsTemplate = "[...$this, ...$0]")]

// list.Insert(index, item) → immutableInsert(list, index, item)
[assembly: MapMethod(typeof(ImmutableList<>), nameof(ImmutableList<int>.Insert),
    JsTemplate = "immutableInsert($this, $0, $1)",
    RuntimeImports = "immutableInsert")]

// list.RemoveAt(index) → immutableRemoveAt(list, index)
[assembly: MapMethod(typeof(ImmutableList<>), nameof(ImmutableList<int>.RemoveAt),
    JsTemplate = "immutableRemoveAt($this, $0)",
    RuntimeImports = "immutableRemoveAt")]

// list.Remove(item) → immutableRemove(list, item)
// The helper returns the original array reference when the item is not found,
// matching ImmutableList<T>.Remove which can safely share immutable instances.
[assembly: MapMethod(typeof(ImmutableList<>), nameof(ImmutableList<int>.Remove),
    JsTemplate = "immutableRemove($this, $0)",
    RuntimeImports = "immutableRemove")]

// list.Clear() → []
[assembly: MapMethod(typeof(ImmutableList<>), nameof(ImmutableList<int>.Clear),
    JsTemplate = "[]")]

// ImmutableList<T>.Empty (static property) → [] — the receiver (the type identifier)
// is dropped because the template doesn't reference $this.
[assembly: MapProperty(typeof(ImmutableList<>), nameof(ImmutableList<int>.Empty),
    JsTemplate = "[]")]

// list.Count and IndexOf reuse the JS array shape directly.
[assembly: MapProperty(typeof(ImmutableList<>), nameof(ImmutableList<int>.Count),
    JsProperty = "length")]
[assembly: MapMethod(typeof(ImmutableList<>), nameof(ImmutableList<int>.IndexOf),
    JsMethod = "indexOf")]
[assembly: MapMethod(typeof(ImmutableList<>), nameof(ImmutableList<int>.Contains),
    JsMethod = "includes")]

// ─── ImmutableArray<T> ──────────────────────────────────────
// Same shape as ImmutableList<T> at the JS level, just a different declaring type so
// each declaration has to repeat. (Could be deduplicated if [MapMethod] grew a
// "DeclaringTypes" array property — out of scope for now.)

[assembly: MapMethod(typeof(ImmutableArray<>), nameof(ImmutableArray<int>.Add),
    JsTemplate = "[...$this, $0]")]

[assembly: MapMethod(typeof(ImmutableArray<>), nameof(ImmutableArray<int>.AddRange),
    JsTemplate = "[...$this, ...$0]")]

[assembly: MapMethod(typeof(ImmutableArray<>), nameof(ImmutableArray<int>.Insert),
    JsTemplate = "immutableInsert($this, $0, $1)",
    RuntimeImports = "immutableInsert")]

[assembly: MapMethod(typeof(ImmutableArray<>), nameof(ImmutableArray<int>.RemoveAt),
    JsTemplate = "immutableRemoveAt($this, $0)",
    RuntimeImports = "immutableRemoveAt")]

[assembly: MapMethod(typeof(ImmutableArray<>), nameof(ImmutableArray<int>.Remove),
    JsTemplate = "immutableRemove($this, $0)",
    RuntimeImports = "immutableRemove")]

[assembly: MapMethod(typeof(ImmutableArray<>), nameof(ImmutableArray<int>.Clear),
    JsTemplate = "[]")]

// ImmutableArray<T>.Empty (static field) → []
[assembly: MapProperty(typeof(ImmutableArray<>), nameof(ImmutableArray<int>.Empty),
    JsTemplate = "[]")]

[assembly: MapProperty(typeof(ImmutableArray<>), nameof(ImmutableArray<int>.Length),
    JsProperty = "length")]
[assembly: MapMethod(typeof(ImmutableArray<>), nameof(ImmutableArray<int>.IndexOf),
    JsMethod = "indexOf")]
[assembly: MapMethod(typeof(ImmutableArray<>), nameof(ImmutableArray<int>.Contains),
    JsMethod = "includes")]
