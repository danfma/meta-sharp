/** biome-ignore-all lint/complexity/noUselessConstructor: explicit shape preserved by transpiler */
import { type Signal as IRawSignal } from "solid-js";
import type { ISignal } from "./i-signal";

export class SignalWrapper<T> implements ISignal<T> {
  private readonly _signal: IRawSignal<T>;

  constructor(signal: IRawSignal<T>) {
    this._signal = signal;
  }

  get value(): T {
    return this._signal[0]();
  }

  set value(value: T) {
    this.set(value);
  }

  private setValue(value: T): void {
    this._signal[1]((_: T) => value);
  }

  private setUpdater(updater: (arg: T) => T): void {
    this._signal[1](updater);
  }

  set(value: T): void;
  set(updater: (arg: T) => T): void;
  set(...args: unknown[]): void {
    if (args.length === 1 && typeof args[0] === "object") {
      this.setValue(args[0] as T);

      return;
    }

    if (args.length === 1 && typeof args[0] === "function" && args[0].length === 1) {
      this.setUpdater(args[0] as (arg: T) => T);

      return;
    }

    throw new Error("No matching overload for set");
  }
}
