import { Component } from "inferno";
import { Counter } from "#/models";
import { Button, Column, Heading, Row } from "#/mvu";
export class CounterApp extends Component {
    constructor() {
        super();
    }
    render() {
        return Column({
            gap: 12,
            children: [Heading({ content: `Count: ${this.state.count}` }), Row({
                    gap: 8,
                    children: [Button({
                            label: "➖",
                            onClick: () => this.setState(this.state.decrement()),
                        }), Button({
                            label: "➕",
                            onClick: () => this.setState(this.state.increment()),
                        }), Button({
                            label: "Reset",
                            onClick: () => this.setState(Counter.zero),
                        })],
                })],
        });
    }
}
