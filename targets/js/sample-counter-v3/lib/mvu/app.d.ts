import type { IWidget } from "./i-widget";
export declare class App {
    constructor();
    static mount<TState>(containerId: string, initialState: TState, view: (state: TState, setState: (obj: TState) => void) => IWidget): void;
    private static apply;
    private static resolveContainer;
}
