/** biome-ignore-all lint/complexity/noUselessConstructor: explicit shape preserved by transpiler */
import { createSignal, type ISignal } from "#/js-interop/solid-js";
import { Counter } from "#/models";
import { createEffect } from "solid-js";

export class CounterStore {
  private readonly _counter: ISignal<Counter>;

  constructor() {
    this._counter = createSignal(Counter.zero);
    createEffect(() => {
      console.log(`Counter has changed: ${this.state().count}`);
    });
  }

  state(): Counter {
    return this._counter.value;
  }

  increment(): void {
    this._counter.set((x: Counter) => x.increment());
  }

  decrement(): void {
    this._counter.set((x: Counter) => x.decrement());
  }

  static create(): CounterStore {
    return new CounterStore();
  }
}
