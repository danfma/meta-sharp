import type { IView } from "./i-view";
export declare class Renderer {
    private readonly _container;
    constructor(containerId: string);
    private createElement;
    render(view: IView): void;
}
