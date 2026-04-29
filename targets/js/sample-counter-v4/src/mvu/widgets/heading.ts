import { Widget } from "#/mvu";

export class Heading extends Widget {
  private readonly _level: number;

  private readonly _content: string;

  constructor(content: string, level: number) {
    super();
    this._content = content;
    this._level = level;
  }

  render(): HTMLElement {
    const sizeEm = this._level === 1 ? 2 : this._level === 2 ? 1.5 : this._level === 3 ? 1.25 : this._level === 4 ? 1 : this._level === 5 ? 0.875 : 0.75;
    const span = document.createElement("span");
    span.textContent = this._content;
    span.setAttribute('style', `display:block;font-weight:bold;font-size:${sizeEm}em`);

    return span;
  }
}
