import { Counter } from "#/models";
import { App, StatefulWidget, button, column, heading, row, type BuildContext, type Widget } from "#/mvu";

export class CounterApp extends StatefulWidget<Counter> {
  constructor() {
    super();
  }

  initial(): Counter {
    return Counter.zero;
  }

  protected build(ctx: BuildContext<Counter>): Widget {
    return column({
      gap: 12,
      children: [heading({ content: `Count: ${ctx.state.count}` }), row({
        gap: 8,
        children: [button({
          label: "➖",
          onPressed: () => ctx.setState((s: Counter) => s.decrement()),
        }), button({
          label: "➕",
          onPressed: () => ctx.setState((s: Counter) => s.increment()),
        }), button({
          label: "Reset",
          onPressed: () => ctx.setState((_: Counter) => Counter.zero),
        })],
      })],
    });
  }

  static mount(containerId: string): void {
    App.run(containerId, new CounterApp());
  }
}
