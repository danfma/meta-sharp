// Declarative BCL → JavaScript mappings for System.String.
//
// Most are straightforward renames between the C# method and the JS String prototype
// equivalent. Methods absent from the JS string prototype (PadLeft/PadRight, Format, etc.)
// are not mapped here yet — they'll need either templates or runtime helpers.

using MetaSharp.Annotations;

// ─── string property ────────────────────────────────────────

[assembly: MapProperty(typeof(string), "Length", JsProperty = "length")]

// ─── Case conversion ────────────────────────────────────────
// JS string has no locale variants; both invariant and culture-aware C# helpers map to
// the same JS counterpart.

[assembly: MapMethod(typeof(string), "ToUpper", JsMethod = "toUpperCase")]
[assembly: MapMethod(typeof(string), "ToUpperInvariant", JsMethod = "toUpperCase")]
[assembly: MapMethod(typeof(string), "ToLower", JsMethod = "toLowerCase")]
[assembly: MapMethod(typeof(string), "ToLowerInvariant", JsMethod = "toLowerCase")]

// ─── Search / inspection ────────────────────────────────────

[assembly: MapMethod(typeof(string), "Contains", JsMethod = "includes")]
[assembly: MapMethod(typeof(string), "StartsWith", JsMethod = "startsWith")]
[assembly: MapMethod(typeof(string), "EndsWith", JsMethod = "endsWith")]
[assembly: MapMethod(typeof(string), "IndexOf", JsMethod = "indexOf")]

// ─── Trimming ───────────────────────────────────────────────

[assembly: MapMethod(typeof(string), "Trim", JsMethod = "trim")]
[assembly: MapMethod(typeof(string), "TrimStart", JsMethod = "trimStart")]
[assembly: MapMethod(typeof(string), "TrimEnd", JsMethod = "trimEnd")]

// ─── Substring / replace ────────────────────────────────────

[assembly: MapMethod(typeof(string), "Substring", JsMethod = "substring")]
[assembly: MapMethod(typeof(string), "Replace", JsMethod = "replace")]
