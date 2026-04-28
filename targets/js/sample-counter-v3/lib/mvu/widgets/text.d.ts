import type { IWidget } from "#/mvu";
export declare class Text implements IWidget {
    private readonly _content;
    constructor(content: string);
    build(): HTMLElement;
}
