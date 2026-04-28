import type { IWidget } from "#/mvu";
export declare class Column implements IWidget {
    private readonly _children;
    constructor(children: IWidget[]);
    build(): HTMLElement;
}
