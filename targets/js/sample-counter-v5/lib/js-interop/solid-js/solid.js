import { SignalWrapper } from "./signal-wrapper";
import { createSignal as createRawSignal } from "solid-js";
export function createSignal(value) {
    const signal = createRawSignal(value);
    return new SignalWrapper(signal);
}
