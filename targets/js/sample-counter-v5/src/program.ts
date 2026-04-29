import { CounterApp } from "#/components";
import { createElement } from "inferno-create-element";
import { render } from "inferno";

render(createElement(CounterApp, {}), (document.getElementById("root") ?? (() => { const el = document.createElement("div"); el.id = "root"; document.body.append(el); return el; })()));
