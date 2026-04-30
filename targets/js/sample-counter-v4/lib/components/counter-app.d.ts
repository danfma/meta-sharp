/** biome-ignore-all lint/complexity/noUselessConstructor: explicit shape preserved by transpiler */
import { Component, type VNode as InfernoElement } from "inferno";
import type { EmptyProps } from "#/inferno";
import { Counter } from "#/models";
export declare class CounterApp extends Component<EmptyProps, Counter> {
    constructor();
    render(): InfernoElement;
}
