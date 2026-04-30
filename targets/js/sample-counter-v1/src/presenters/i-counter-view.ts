/** biome-ignore-all lint/complexity/noUselessConstructor: explicit shape preserved by transpiler */
import type { IView } from "#/views";

export interface ICounterView extends IView {
  onButtonClick: (() => void) | null;
  showCounter(counter: number): void;
}
