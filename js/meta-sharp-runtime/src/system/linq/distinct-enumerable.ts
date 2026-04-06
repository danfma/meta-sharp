import { EnumerableBase } from "./enumerable-base.ts";

/** Returns distinct elements using Set-based deduplication. */
export class DistinctEnumerable<T> extends EnumerableBase<T> {
  constructor(readonly source: EnumerableBase<T>) {
    super();
  }

  *[Symbol.iterator](): Iterator<T> {
    const seen = new Set<T>();
    for (const item of this.source) {
      if (!seen.has(item)) {
        seen.add(item);
        yield item;
      }
    }
  }
}
