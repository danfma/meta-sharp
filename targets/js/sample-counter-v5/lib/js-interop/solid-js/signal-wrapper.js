export class SignalWrapper {
    _signal;
    constructor(signal) {
        this._signal = signal;
    }
    get value() {
        return this._signal[0]();
    }
    set value(value) {
        this.set(value);
    }
    setValue(value) {
        this._signal[1]((_) => value);
    }
    setUpdater(updater) {
        this._signal[1](updater);
    }
    set(...args) {
        if (args.length === 1 && typeof args[0] === "object") {
            this.setValue(args[0]);
            return;
        }
        if (args.length === 1 && typeof args[0] === "function" && args[0].length === 1) {
            this.setUpdater(args[0]);
            return;
        }
        throw new Error("No matching overload for set");
    }
}
