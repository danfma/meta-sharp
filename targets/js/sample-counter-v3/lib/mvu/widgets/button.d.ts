import type { IWidget } from "#/mvu";
export declare class Button implements IWidget {
    private readonly _label;
    private readonly _onPressed;
    constructor(label: string, onPressed: () => void);
    build(): HTMLElement;
}
