// Override of the generated CounterApp module. Two issues block the
// generated output today:
//   - Inferno's `Component<P, S>` declares `state: S | null`; Metano
//     drops the C# null-forgiving operator (`State!.Count`) so the
//     emitted body fails strict-null checks.
//   - `[Emit]` template type-arg placeholders (#189) aren't shipped
//     yet; once they are, this file goes away.
import { Component } from "inferno";
import { createElement } from "inferno-create-element";
import { Counter } from "#/models";
import { Button, Column, Heading, Row } from "#/mvu";
export class CounterApp extends Component {
    constructor() {
        super();
        this.state = Counter.zero;
    }
    render() {
        const state = this.state;
        return Column({
            gap: 12,
            children: [
                Heading({ content: `Count: ${state.count}` }),
                Row({
                    gap: 8,
                    children: [
                        Button({
                            label: "➖",
                            onClick: () => this.setState(state.decrement()),
                        }),
                        Button({
                            label: "➕",
                            onClick: () => this.setState(state.increment()),
                        }),
                        Button({
                            label: "Reset",
                            onClick: () => this.setState(Counter.zero),
                        }),
                    ],
                }),
            ],
        });
    }
}
