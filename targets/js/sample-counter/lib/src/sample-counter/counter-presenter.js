import { Counter } from "./counter";
export class CounterPresenter {
    _view;
    _counter = Counter.zero;
    constructor(view) {
        this._view = view;
        this.initialize();
    }
    initialize() {
        this._view.displayCounter(this._counter);
    }
    increment() {
        this._counter = this._counter.increment();
        this.displayCounter();
    }
    decrement() {
        this._counter = this._counter.decrement();
        this.displayCounter();
    }
    displayCounter() {
        this._view.displayCounter(this._counter);
    }
}
