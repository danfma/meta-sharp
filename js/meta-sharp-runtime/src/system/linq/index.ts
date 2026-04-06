// Register factory functions to break circular dependency between
// EnumerableBase (which needs to create subclasses) and the subclasses
// (which extend EnumerableBase).
import { _registerFactories } from "./enumerable-base.ts";
import { WhereEnumerable } from "./where-enumerable.ts";
import { SelectEnumerable } from "./select-enumerable.ts";
import { SelectManyEnumerable } from "./select-many-enumerable.ts";
import { OrderByEnumerable } from "./order-by-enumerable.ts";
import { TakeEnumerable } from "./take-enumerable.ts";
import { SkipEnumerable } from "./skip-enumerable.ts";
import { DistinctEnumerable } from "./distinct-enumerable.ts";
import { GroupByEnumerable } from "./group-by-enumerable.ts";
import { ConcatEnumerable } from "./concat-enumerable.ts";

_registerFactories({
  where: (source, predicate) => new WhereEnumerable(source, predicate),
  select: (source, selector) => new SelectEnumerable(source, selector),
  selectMany: (source, selector) => new SelectManyEnumerable(source, selector),
  orderBy: (source, keySelector, descending) => new OrderByEnumerable(source, keySelector, descending),
  take: (source, count) => new TakeEnumerable(source, count),
  skip: (source, count) => new SkipEnumerable(source, count),
  distinct: (source) => new DistinctEnumerable(source),
  groupBy: (source, keySelector) => new GroupByEnumerable(source, keySelector),
  concat: (first, second) => new ConcatEnumerable(first, second),
});

// Re-export everything
export { EnumerableBase, type Grouping } from "./enumerable-base.ts";
export { Enumerable } from "./enumerable.ts";
export { SourceEnumerable } from "./source-enumerable.ts";
export { WhereEnumerable } from "./where-enumerable.ts";
export { SelectEnumerable } from "./select-enumerable.ts";
export { SelectManyEnumerable } from "./select-many-enumerable.ts";
export { OrderByEnumerable } from "./order-by-enumerable.ts";
export { TakeEnumerable } from "./take-enumerable.ts";
export { SkipEnumerable } from "./skip-enumerable.ts";
export { DistinctEnumerable } from "./distinct-enumerable.ts";
export { GroupByEnumerable } from "./group-by-enumerable.ts";
export { ConcatEnumerable } from "./concat-enumerable.ts";
