import type { IView } from "./i-view";

export class Renderer {
  private readonly _container: HTMLElement;

  constructor(containerId: string) {
    this._container = Js.document.getElementById(containerId) ?? this.createElement(containerId);
  }

  private createElement(containerId: string): HTMLElement {
    const element = Js.document.createElement();
    element.id = containerId;
    Js.document.body.append(element);

    return element;
  }

  render(view: IView): void {
  }
}
