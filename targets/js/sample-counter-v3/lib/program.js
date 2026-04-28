import { Counter } from "#/models";
import { App } from "#/mvu";
import { Button, Column, Text } from "#/mvu/widgets";
App.mount("root", Counter.zero, (state, setState) => new Column([new Text(state.count.toString()), new Button("Click me", () => setState(state.increment()))]));
