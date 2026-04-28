export declare class StateHolder<TState> {
    private _state;
    onChange: (() => void) | null;
    constructor(initial: TState);
    get state(): TState;
    set(next: TState): void;
}
