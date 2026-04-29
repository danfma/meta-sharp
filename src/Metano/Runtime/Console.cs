// Declarative BCL → target mappings for System.Console.
//
// JS: WriteLine → console.log($0) via template (the JS console object is a
// lowercase identifier, not a rename of `Console`).
// Dart: WriteLine → top-level `print(...)` function. Since `print` is a static
// function (no receiver), the simple `DartMethod` rename suffices — the bridge
// drops the `Console.` qualifier for static calls.

using Metano.Annotations;

[assembly: MapMethod(
    typeof(Console),
    nameof(Console.WriteLine),
    JsTemplate = "console.log($0)",
    DartMethod = "print"
)]
