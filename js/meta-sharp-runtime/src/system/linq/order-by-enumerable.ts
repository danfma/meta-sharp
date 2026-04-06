import { EnumerableBase } from "./enumerable-base.ts";

/** Sorts elements by a key. Materializes the source on first iteration. */
export class OrderByEnumerable<T, K> extends EnumerableBase<T> {
  constructor(
    readonly source: EnumerableBase<T>,
    readonly keySelector: (item: T) => K,
    readonly descending: boolean,
  ) {
    super();
  }

  *[Symbol.iterator](): Iterator<T> {
    const items = [...this.source];
    items.sort((a, b) => {
      const ka = this.keySelector(a);
      const kb = this.keySelector(b);
      const cmp = ka < kb ? -1 : ka > kb ? 1 : 0;
      return this.descending ? -cmp : cmp;
    });
    yield* items;
  }
}
