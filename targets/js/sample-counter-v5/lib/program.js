import { getOrCreateElementById } from "#/bindings";
import { CounterApp } from "#/components";
import { createElement } from "inferno-create-element";
import { render } from "inferno";
render(createElement(CounterApp, {}), getOrCreateElementById(document, "root"));
