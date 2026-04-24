import { Renderer } from "#/views";
export class CounterPresenter {
    _view;
    _state;
    constructor(view, initialState) {
        this._view = view;
        this._state = initialState;
    }
    startApplication(containerName) {
        const renderer = new Renderer(containerName);
        renderer.render(this._view);
        this._view.onButtonClick = this.onButtonClicked.bind(this);
        this._view.showCounter(this._state.count);
    }
    onButtonClicked() {
        this._state = this._state.increment();
        this._view.showCounter(this._state.count);
    }
}
