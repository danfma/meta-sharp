import { HashCode } from "metano-runtime";

export class Counter {
  readonly count: number = 0;

  constructor() { }

  equals(other: any): boolean {
    return other instanceof Counter;
  }

  hashCode(): number {
    const hc = new HashCode();

    return hc.toHashCode();
  }

  with(overrides?: Partial<Counter>): Counter {
    return new Counter();
  }
}
