import type { IView } from "#/views";

export interface ICounterView extends IView {
  onButtonClick: (() => void) | null;
  showCounter(counter: number): void;
}
