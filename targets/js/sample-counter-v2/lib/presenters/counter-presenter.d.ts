import type { Counter } from "#/models";
import type { ICounterView } from "./i-counter-view";
export declare class CounterPresenter {
    private _state;
    private readonly _view;
    constructor(view: ICounterView, initialState: Counter);
    startApplication(containerName: string): void;
    private onButtonClicked;
}
