import type { Counter } from "#/models";
import { Renderer } from "#/views";
import type { ICounterView } from "./i-counter-view";

export class CounterPresenter {
  private readonly _view: ICounterView;
  private _state: Counter;

  constructor(view: ICounterView, initialState: Counter) {
    this._view = view;
    this._state = initialState;
  }

  startApplication(containerName: string): void {
    const renderer = new Renderer(containerName);
    renderer.render(this._view);
    this._view.onButtonClick = this.onButtonClicked.bind(this);
    this._view.showCounter(this._state.count);
  }

  private onButtonClicked(): void {
    this._state = this._state.increment();
    this._view.showCounter(this._state.count);
  }
}
