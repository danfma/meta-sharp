export class StateHolder<TState> {
  private _state: TState;

  onChange: (() => void) | null = null;

  constructor(initial: TState) {
    this._state = initial;
  }

  get state(): TState {
    return this._state;
  }

  set(next: TState): void {
    this._state = next;
    this.onChange?.();
  }
}
