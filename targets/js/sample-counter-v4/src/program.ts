/** biome-ignore-all lint/complexity/noUselessConstructor: explicit shape preserved by transpiler */
import { CounterApp } from "#/components";
import { getOrCreateElementById } from "#/inferno";
import { createElement } from "inferno-create-element";
import { render } from "inferno";

render(createElement(CounterApp, {}), getOrCreateElementById(document, "root"));
