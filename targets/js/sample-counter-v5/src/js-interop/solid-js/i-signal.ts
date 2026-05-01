/** biome-ignore-all lint/complexity/noUselessConstructor: explicit shape preserved by transpiler */
export interface ISignal<T> {
  readonly value: T;
  set(value: T): void;
  set(updater: (arg: T) => T): void;
}
