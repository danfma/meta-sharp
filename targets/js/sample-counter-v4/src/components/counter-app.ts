import { Counter } from "#/models";
import { App, StatefulWidget, UI, type BuildContext, type Widget } from "#/mvu";

export class CounterApp extends StatefulWidget<Counter> {
  constructor() {
    super();
  }

  initial(): Counter {
    return Counter.zero;
  }

  protected build(ctx: BuildContext<Counter>): Widget {
    return UI.column(12, [UI.heading(`Count: ${ctx.state.count}`, 1), UI.row(8, [UI.button("➖", () => ctx.setState((s: Counter) => s.decrement())), UI.button("➕", () => ctx.setState((s: Counter) => s.increment())), UI.button("Reset", () => ctx.setState((_: Counter) => Counter.zero))])]);
  }

  static mount(containerId: string): void {
    App.run(containerId, new CounterApp());
  }
}
