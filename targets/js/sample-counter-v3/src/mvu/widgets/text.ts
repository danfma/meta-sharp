import type { IWidget } from "#/mvu";

export class Text implements IWidget {
  private readonly _content: string;

  constructor(content: string) {
    this._content = content;
  }

  build(): HTMLElement {
    const span = document.createElement("span");
    span.innerHTML = this._content;

    return span;
  }
}
