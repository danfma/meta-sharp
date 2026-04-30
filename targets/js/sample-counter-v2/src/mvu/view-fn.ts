import type { IWidget } from "./i-widget";

export type ViewFn<TState> = (state: TState, setState: (obj: TState) => void) => IWidget;
