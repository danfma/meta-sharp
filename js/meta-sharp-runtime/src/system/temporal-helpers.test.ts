import { describe, expect, test } from "bun:test";
import { Temporal } from "@js-temporal/polyfill";
import { dayNumber } from "./temporal-helpers.ts";

describe("dayNumber", () => {
  // Reference values from C# DateOnly.DayNumber:
  //   new DateOnly(0001, 01, 01).DayNumber → 0
  //   new DateOnly(0001, 01, 02).DayNumber → 1
  //   new DateOnly(0001, 12, 31).DayNumber → 364
  //   new DateOnly(0002, 01, 01).DayNumber → 365
  //   new DateOnly(1970, 01, 01).DayNumber → 719162
  //   new DateOnly(2000, 01, 01).DayNumber → 730119
  //   new DateOnly(2024, 03, 15).DayNumber → 738959
  //   new DateOnly(2026, 04, 06).DayNumber → 739711
  //   new DateOnly(9999, 12, 31).DayNumber → 3652058

  test("epoch: 0001-01-01 → 0", () => {
    expect(dayNumber(Temporal.PlainDate.from("0001-01-01"))).toBe(0);
  });

  test("0001-01-02 → 1", () => {
    expect(dayNumber(Temporal.PlainDate.from("0001-01-02"))).toBe(1);
  });

  test("end of first year: 0001-12-31 → 364", () => {
    expect(dayNumber(Temporal.PlainDate.from("0001-12-31"))).toBe(364);
  });

  test("start of second year: 0002-01-01 → 365", () => {
    expect(dayNumber(Temporal.PlainDate.from("0002-01-01"))).toBe(365);
  });

  test("unix epoch: 1970-01-01 → 719162", () => {
    expect(dayNumber(Temporal.PlainDate.from("1970-01-01"))).toBe(719162);
  });

  test("Y2K: 2000-01-01 → 730119", () => {
    expect(dayNumber(Temporal.PlainDate.from("2000-01-01"))).toBe(730119);
  });

  test("2024-03-15 → 738959", () => {
    expect(dayNumber(Temporal.PlainDate.from("2024-03-15"))).toBe(738959);
  });

  test("today 2026-04-06 → 739711", () => {
    expect(dayNumber(Temporal.PlainDate.from("2026-04-06"))).toBe(739711);
  });

  test("max: 9999-12-31 → 3652058", () => {
    expect(dayNumber(Temporal.PlainDate.from("9999-12-31"))).toBe(3652058);
  });

  test("subtraction matches duration in days", () => {
    const start = Temporal.PlainDate.from("2026-01-01");
    const end = Temporal.PlainDate.from("2026-01-31");
    expect(dayNumber(end) - dayNumber(start)).toBe(30);
  });

  test("leap year: 2024-02-29 exists and is consistent", () => {
    const feb28 = Temporal.PlainDate.from("2024-02-28");
    const feb29 = Temporal.PlainDate.from("2024-02-29");
    const mar01 = Temporal.PlainDate.from("2024-03-01");
    expect(dayNumber(feb29) - dayNumber(feb28)).toBe(1);
    expect(dayNumber(mar01) - dayNumber(feb29)).toBe(1);
  });
});
