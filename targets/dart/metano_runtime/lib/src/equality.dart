import 'hash_code.dart';

/// Deep value-equality and content-based hashing helpers for arbitrary values.
///
/// These mirror the TypeScript runtime helpers in `metano-runtime/equality.ts`.
/// They are NOT called for trivial cases — Metano-generated records use their
/// own inline `operator ==` / `hashCode`, and Dart's primitive types already
/// compare by value. Reach for [deepEquals] / [deepHashCode] when comparing
/// arbitrary structures where one side is a `List`, `Map`, or `Set` whose
/// `==` is identity-based by default.

/// Returns true when [a] and [b] are deeply equal. Order of checks:
///
/// 1. Reference identity wins.
/// 2. `null` compares equal only to `null`.
/// 3. Two `double` NaNs are treated as equal (parity with C# `double.NaN.Equals`).
/// 4. `List`s are equal element-wise (in order).
/// 5. `Map`s are equal when they have the same size and the same key→value pairs.
/// 6. `Set`s are equal when they have the same size and the same membership.
/// 7. Anything else falls back to `a == b`, so types with overridden `==`
///    (records, branded types, `DateTime`, `String`, primitives) work as expected.
///
/// Cycles in the input graph are NOT detected — callers must pass acyclic data.
bool deepEquals(Object? a, Object? b) {
  if (identical(a, b)) return true;
  if (a == null || b == null) return false;

  if (a is double && b is double && a.isNaN && b.isNaN) return true;

  if (a is List) {
    if (b is! List || a.length != b.length) return false;
    for (var i = 0; i < a.length; i++) {
      if (!deepEquals(a[i], b[i])) return false;
    }
    return true;
  }
  if (b is List) return false;

  if (a is Map) {
    if (b is! Map || a.length != b.length) return false;
    for (final key in a.keys) {
      if (!b.containsKey(key)) return false;
      if (!deepEquals(a[key], b[key])) return false;
    }
    return true;
  }
  if (b is Map) return false;

  if (a is Set) {
    if (b is! Set || a.length != b.length) return false;
    for (final item in a) {
      if (!b.contains(item)) return false;
    }
    return true;
  }
  if (b is Set) return false;

  return a == b;
}

/// Computes a content-based hash for an arbitrary value, consistent with
/// [deepEquals] — values that compare equal produce the same hash.
///
/// Maps and Sets fold their elements with XOR so the hash is independent of
/// iteration order, matching how their equality is defined above.
int deepHashCode(Object? value) {
  if (value == null) return 0;

  if (value is List) {
    final hc = HashCode()..add(value.length);
    for (final item in value) {
      hc.add(deepHashCode(item));
    }
    return hc.toHashCode();
  }

  if (value is Map) {
    var acc = 0;
    for (final entry in value.entries) {
      acc ^=
          HashCode.combine2(deepHashCode(entry.key), deepHashCode(entry.value));
    }
    return acc;
  }

  if (value is Set) {
    var acc = 0;
    for (final item in value) {
      acc ^= deepHashCode(item);
    }
    return acc;
  }

  if (value is num || value is String || value is bool || value is BigInt) {
    return HashCode.combine(value);
  }

  return value.hashCode;
}
