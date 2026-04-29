// Hand-written entry override. The C# `Program.Main` body relies on
// `typeof(CounterApp)` and a literal Inferno class reference; neither
// pattern lowers cleanly today (TypeOfExpression isn't modeled by the
// IR pipeline yet, and `[Emit]` templates don't drive imports). This
// file replaces the generated stub so the bundler picks up real
// references to both the npm package and the transpiled component.
import { createElement } from "inferno-create-element";
import { render } from "inferno";
import { CounterApp } from "#/components";
const container = document.getElementById("root") ??
    (() => {
        const element = document.createElement("div");
        element.id = "root";
        document.body.append(element);
        return element;
    })();
render(createElement(CounterApp, {}), container);
