import type { ICounterView } from "#/presenters";
export declare class CounterView implements ICounterView {
    private _text;
    onButtonClick: (() => void) | null;
    constructor();
    render(container: HTMLElement): void;
    showCounter(counter: number): void;
}
