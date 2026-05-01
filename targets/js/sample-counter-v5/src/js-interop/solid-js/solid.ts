/** biome-ignore-all lint/complexity/noUselessConstructor: explicit shape preserved by transpiler */
import type { ISignal } from "./i-signal";
import { SignalWrapper } from "./signal-wrapper";
import { createSignal as createRawSignal } from "solid-js";

export function createSignal<T>(value: T): ISignal<T> {
  const signal = createRawSignal(value);

  return new SignalWrapper(signal);
}
