import type { Counter } from "#/models";
import type { ICounterView } from "./i-counter-view";
export declare class CounterPresenter {
    private readonly _view;
    private _state;
    constructor(view: ICounterView, initialState: Counter);
    startApplication(containerName: string): void;
    private onButtonClicked;
}
