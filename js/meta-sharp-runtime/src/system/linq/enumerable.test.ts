import { describe, expect, test } from "bun:test";
import { Enumerable } from "./index.ts";

describe("Enumerable", () => {
  // ─── Factories ──────────────────────────────────────────

  test("from wraps an array", () => {
    const result = Enumerable.from([1, 2, 3]).toArray();
    expect(result).toEqual([1, 2, 3]);
  });

  test("empty returns empty sequence", () => {
    expect(Enumerable.empty<number>().toArray()).toEqual([]);
  });

  test("range generates sequence", () => {
    expect(Enumerable.range(5, 3).toArray()).toEqual([5, 6, 7]);
  });

  test("repeat generates repeated elements", () => {
    expect(Enumerable.repeat("x", 3).toArray()).toEqual(["x", "x", "x"]);
  });

  // ─── Lazy composition ─────────────────────────────────

  test("where filters elements", () => {
    const result = Enumerable.from([1, 2, 3, 4, 5])
      .where(x => x % 2 === 0)
      .toArray();
    expect(result).toEqual([2, 4]);
  });

  test("select projects elements", () => {
    const result = Enumerable.from([1, 2, 3])
      .select(x => x * 10)
      .toArray();
    expect(result).toEqual([10, 20, 30]);
  });

  test("selectMany flattens", () => {
    const result = Enumerable.from(["hello", "world"])
      .selectMany(s => s.split(""))
      .toArray();
    expect(result).toEqual(["h", "e", "l", "l", "o", "w", "o", "r", "l", "d"]);
  });

  test("where + select chain is lazy", () => {
    let sideEffects = 0;
    const query = Enumerable.from([1, 2, 3, 4, 5])
      .where(x => { sideEffects++; return x > 2; })
      .select(x => x * 10);

    // No iteration yet — should have zero side effects
    expect(sideEffects).toBe(0);

    const result = query.toArray();
    expect(result).toEqual([30, 40, 50]);
    expect(sideEffects).toBe(5); // all elements checked by where
  });

  test("orderBy sorts ascending", () => {
    const result = Enumerable.from([3, 1, 4, 1, 5])
      .orderBy(x => x)
      .toArray();
    expect(result).toEqual([1, 1, 3, 4, 5]);
  });

  test("orderByDescending sorts descending", () => {
    const result = Enumerable.from([3, 1, 4])
      .orderByDescending(x => x)
      .toArray();
    expect(result).toEqual([4, 3, 1]);
  });

  test("take returns first N elements", () => {
    const result = Enumerable.from([1, 2, 3, 4, 5])
      .take(3)
      .toArray();
    expect(result).toEqual([1, 2, 3]);
  });

  test("skip skips first N elements", () => {
    const result = Enumerable.from([1, 2, 3, 4, 5])
      .skip(2)
      .toArray();
    expect(result).toEqual([3, 4, 5]);
  });

  test("distinct removes duplicates", () => {
    const result = Enumerable.from([1, 2, 2, 3, 1, 3])
      .distinct()
      .toArray();
    expect(result).toEqual([1, 2, 3]);
  });

  test("concat joins two sequences", () => {
    const result = Enumerable.from([1, 2])
      .concat([3, 4])
      .toArray();
    expect(result).toEqual([1, 2, 3, 4]);
  });

  test("groupBy groups elements", () => {
    const result = Enumerable.from(["apple", "avocado", "banana", "blueberry"])
      .groupBy(s => s[0]!)
      .select(g => ({ key: g.key, items: [...g] }))
      .toArray();

    expect(result).toEqual([
      { key: "a", items: ["apple", "avocado"] },
      { key: "b", items: ["banana", "blueberry"] },
    ]);
  });

  // ─── Terminal operations ──────────────────────────────

  test("first returns first matching element", () => {
    expect(Enumerable.from([1, 2, 3]).first()).toBe(1);
    expect(Enumerable.from([1, 2, 3]).first(x => x > 1)).toBe(2);
  });

  test("first throws on empty", () => {
    expect(() => Enumerable.empty<number>().first()).toThrow();
  });

  test("firstOrDefault returns null on empty", () => {
    expect(Enumerable.empty<number>().firstOrDefault()).toBeNull();
    expect(Enumerable.from([1, 2]).firstOrDefault(x => x > 5)).toBeNull();
  });

  test("last returns last matching", () => {
    expect(Enumerable.from([1, 2, 3]).last()).toBe(3);
    expect(Enumerable.from([1, 2, 3]).last(x => x < 3)).toBe(2);
  });

  test("single returns the only element", () => {
    expect(Enumerable.from([42]).single()).toBe(42);
  });

  test("single throws on multiple", () => {
    expect(() => Enumerable.from([1, 2]).single()).toThrow();
  });

  test("any checks existence", () => {
    expect(Enumerable.from([1, 2, 3]).any()).toBe(true);
    expect(Enumerable.empty<number>().any()).toBe(false);
    expect(Enumerable.from([1, 2, 3]).any(x => x > 2)).toBe(true);
    expect(Enumerable.from([1, 2, 3]).any(x => x > 5)).toBe(false);
  });

  test("all checks all elements", () => {
    expect(Enumerable.from([2, 4, 6]).all(x => x % 2 === 0)).toBe(true);
    expect(Enumerable.from([2, 3, 6]).all(x => x % 2 === 0)).toBe(false);
  });

  test("count counts elements", () => {
    expect(Enumerable.from([1, 2, 3]).count()).toBe(3);
    expect(Enumerable.from([1, 2, 3]).count(x => x > 1)).toBe(2);
  });

  test("sum sums elements", () => {
    expect(Enumerable.from([1, 2, 3]).sum()).toBe(6);
    expect(Enumerable.from(["a", "bb", "ccc"]).sum(s => s.length)).toBe(6);
  });

  test("min/max", () => {
    expect(Enumerable.from([3, 1, 4]).min()).toBe(1);
    expect(Enumerable.from([3, 1, 4]).max()).toBe(4);
  });

  test("contains checks for element", () => {
    expect(Enumerable.from([1, 2, 3]).contains(2)).toBe(true);
    expect(Enumerable.from([1, 2, 3]).contains(5)).toBe(false);
  });

  test("aggregate folds elements", () => {
    const result = Enumerable.from([1, 2, 3, 4])
      .aggregate(0, (acc, x) => acc + x);
    expect(result).toBe(10);
  });

  test("toMap creates a Map", () => {
    const map = Enumerable.from(["a", "bb", "ccc"])
      .toMap(s => s, s => s.length);
    expect(map.get("a")).toBe(1);
    expect(map.get("ccc")).toBe(3);
  });

  test("toSet creates a Set", () => {
    const set = Enumerable.from([1, 2, 2, 3]).toSet();
    expect(set.size).toBe(3);
  });

  // ─── Complex chains ───────────────────────────────────

  test("complex chain: filter, project, sort, take", () => {
    const people = [
      { name: "Alice", age: 30 },
      { name: "Bob", age: 25 },
      { name: "Charlie", age: 35 },
      { name: "Diana", age: 28 },
    ];

    const result = Enumerable.from(people)
      .where(p => p.age >= 28)
      .orderBy(p => p.age)
      .select(p => p.name)
      .take(2)
      .toArray();

    expect(result).toEqual(["Diana", "Alice"]);
  });

  test("multiple iterations of same query", () => {
    const query = Enumerable.from([1, 2, 3]).where(x => x > 1);

    // Can iterate multiple times
    expect(query.toArray()).toEqual([2, 3]);
    expect(query.toArray()).toEqual([2, 3]);
    expect(query.count()).toBe(2);
  });
});
