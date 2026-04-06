import { EnumerableBase } from "./enumerable-base.ts";

/** Produces the set difference of two sequences. Equivalent to C# .Except() */
export class ExceptEnumerable<T> extends EnumerableBase<T> {
  constructor(
    readonly first: EnumerableBase<T>,
    readonly second: Iterable<T>,
  ) {
    super();
  }

  *[Symbol.iterator](): Iterator<T> {
    const excluded = new Set<T>(this.second);
    const yielded = new Set<T>();
    for (const item of this.first) {
      if (!excluded.has(item) && !yielded.has(item)) {
        yielded.add(item);
        yield item;
      }
    }
  }
}
