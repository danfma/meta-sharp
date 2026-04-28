import type { IWidget } from "./i-widget";
import { StateHolder } from "./state-holder";

export class App {
  constructor() { }

  static mount<TState>(containerId: string, initialState: TState, view: (state: TState, setState: (obj: TState) => void) => IWidget): void {
    const container = App.resolveContainer(containerId);
    const holder = new StateHolder(initialState);
    App.apply(view(holder.state, holder.set.bind(holder)), container);
    holder.onChange = () => App.apply(view(holder.state, holder.set.bind(holder)), container);
  }

  private static apply(root: IWidget, container: HTMLElement): void {
    container.innerHTML = "";
    container.append(root.build());
  }

  private static resolveContainer(containerId: string): HTMLElement {
    const existing = document.getElementById(containerId);

    if (existing != null) {
      return existing;
    }

    const element = document.createElement("div");
    element.id = containerId;
    document.body.append(element);

    return element;
  }
}
