/** biome-ignore-all lint/complexity/noUselessConstructor: explicit shape preserved by transpiler */
import { createSignal } from "#/js-interop/solid-js";
import { Counter } from "#/models";
import { createEffect } from "solid-js";
export class CounterStore {
    _counter;
    constructor() {
        this._counter = createSignal(Counter.zero);
        createEffect(() => {
            console.log(`Counter has changed: ${this.state().count}`);
        });
    }
    state() {
        return this._counter.value;
    }
    increment() {
        this._counter.set((x) => x.increment());
    }
    decrement() {
        this._counter.set((x) => x.decrement());
    }
    static create() {
        return new CounterStore();
    }
}
