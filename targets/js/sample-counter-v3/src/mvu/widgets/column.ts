import type { IWidget } from "#/mvu";

export class Column implements IWidget {
  private readonly _children: IWidget[];

  constructor(children: IWidget[]) {
    this._children = children;
  }

  build(): HTMLElement {
    const div = document.createElement("div");
    for (let i = 0; i < this._children.length; i++) {
      div.append(this._children[i].build());
    }

    return div;
  }
}
