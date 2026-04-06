import { EnumerableBase } from "./enumerable-base.ts";

/** Produces the set intersection of two sequences. Equivalent to C# .Intersect() */
export class IntersectEnumerable<T> extends EnumerableBase<T> {
  constructor(
    readonly first: EnumerableBase<T>,
    readonly second: Iterable<T>,
  ) {
    super();
  }

  *[Symbol.iterator](): Iterator<T> {
    const secondSet = new Set<T>(this.second);
    const yielded = new Set<T>();
    for (const item of this.first) {
      if (secondSet.has(item) && !yielded.has(item)) {
        yielded.add(item);
        yield item;
      }
    }
  }
}
