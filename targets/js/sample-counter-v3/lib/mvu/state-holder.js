export class StateHolder {
    _state;
    onChange = null;
    constructor(initial) {
        this._state = initial;
    }
    get state() {
        return this._state;
    }
    set(next) {
        this._state = next;
        this.onChange?.();
    }
}
