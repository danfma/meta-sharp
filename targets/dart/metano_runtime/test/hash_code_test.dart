import 'package:flutter_test/flutter_test.dart';
import 'package:metano_runtime/metano_runtime.dart';

void main() {
  group('HashCode', () {
    test('is deterministic across instances', () {
      expect(HashCode.combine(42), equals(HashCode.combine(42)));
      expect(
        HashCode.combine2('foo', 7),
        equals(HashCode.combine2('foo', 7)),
      );
    });

    test('builder and combine helpers agree', () {
      final builder = HashCode()
        ..add('foo')
        ..add(7)
        ..add(true);
      final combined = HashCode.combine3('foo', 7, true);
      expect(builder.toHashCode(), equals(combined));
    });

    test('different orderings produce different hashes', () {
      expect(
        HashCode.combine2(1, 2),
        isNot(equals(HashCode.combine2(2, 1))),
      );
    });

    test('null hashes consistently across calls', () {
      // The xxHash final mix means hashing a single null doesn't return 0 —
      // it returns the seed mixed with one zero input. Just check stability.
      expect(HashCode.combine(null), equals(HashCode.combine(null)));
    });

    test('NaN hashes consistently', () {
      expect(
        HashCode.combine(double.nan),
        equals(HashCode.combine(double.nan)),
      );
    });

    test('builder beyond 4 fields uses fallback path', () {
      // Records with 5+ fields use the builder API in generated code; this
      // exercises the rotation through `_round` that combineN never hits.
      final hc = HashCode()
        ..add('a')
        ..add('b')
        ..add('c')
        ..add('d')
        ..add('e');
      expect(hc.toHashCode(), equals(hc.toHashCode()));
      // Different ordering of 5 fields produces different hash.
      final swapped = HashCode()
        ..add('a')
        ..add('b')
        ..add('c')
        ..add('e')
        ..add('d');
      expect(hc.toHashCode(), isNot(equals(swapped.toHashCode())));
    });

    test('matches TypeScript runtime golden values', () {
      // Captured from `metano-runtime/src/system/hash-code.ts` via
      // `bun -e "import { HashCode } from ...; console.log(...)"`.
      // If these diverge, the Dart and TS ports have drifted bit-wise and
      // any cross-stack hashed payloads (e.g. shared cache keys) will break.
      expect(HashCode.combine(42), equals(1161967057));
      expect(HashCode.combine('foo'), equals(-1619627207));
      expect(HashCode.combine2('foo', 7), equals(1752159422));
      expect(HashCode.combine4(1, 2, 3, 4), equals(1410016957));
      expect(HashCode.combine(true), equals(-205818221));
      expect(HashCode.combine(null), equals(148298089));
      final builder5 = HashCode()
        ..add('a')
        ..add('b')
        ..add('c')
        ..add('d')
        ..add('e');
      expect(builder5.toHashCode(), equals(1369397646));
    });
  });

  group('deepEquals', () {
    test('identity and null', () {
      expect(deepEquals(null, null), isTrue);
      expect(deepEquals(1, null), isFalse);
      expect(deepEquals(null, 'x'), isFalse);
      const obj = 'foo';
      expect(deepEquals(obj, obj), isTrue);
    });

    test('NaN equals NaN', () {
      expect(deepEquals(double.nan, double.nan), isTrue);
      expect(deepEquals(0.0, -0.0), isTrue);
    });

    test('lists compare element-wise', () {
      expect(deepEquals([1, 2, 3], [1, 2, 3]), isTrue);
      expect(deepEquals([1, 2, 3], [1, 2]), isFalse);
      expect(deepEquals([1, 2], [2, 1]), isFalse);
      expect(
        deepEquals([
          [1, 2],
          [3, 4],
        ], [
          [1, 2],
          [3, 4],
        ]),
        isTrue,
      );
    });

    test('maps compare structurally and order-independently', () {
      expect(
        deepEquals({'a': 1, 'b': 2}, {'b': 2, 'a': 1}),
        isTrue,
      );
      expect(
        deepEquals({'a': 1, 'b': 2}, {'a': 1, 'b': 3}),
        isFalse,
      );
    });

    test('sets compare by membership', () {
      expect(deepEquals({1, 2, 3}, {3, 2, 1}), isTrue);
      expect(deepEquals({1, 2, 3}, {1, 2}), isFalse);
    });

    test('cross-type comparisons fail', () {
      expect(deepEquals([1, 2], {1, 2}), isFalse);
      expect(deepEquals({'a': 1}, [1]), isFalse);
    });

    test('delegates to operator == for arbitrary objects', () {
      final a = DateTime.utc(2026, 1, 1);
      final b = DateTime.utc(2026, 1, 1);
      expect(deepEquals(a, b), isTrue);
    });
  });

  group('deepHashCode', () {
    test('null hashes to 0', () {
      expect(deepHashCode(null), equals(0));
    });

    test('agrees with deepEquals on lists', () {
      final a = [1, 2, 3];
      final b = [1, 2, 3];
      expect(deepEquals(a, b), isTrue);
      expect(deepHashCode(a), equals(deepHashCode(b)));
    });

    test('map hash is order-independent', () {
      final a = {'a': 1, 'b': 2, 'c': 3};
      final b = {'c': 3, 'a': 1, 'b': 2};
      expect(deepHashCode(a), equals(deepHashCode(b)));
    });

    test('set hash is order-independent', () {
      expect(deepHashCode({1, 2, 3}), equals(deepHashCode({3, 1, 2})));
    });

    test('different content produces different hashes', () {
      expect(
        deepHashCode([1, 2, 3]),
        isNot(equals(deepHashCode([1, 2, 4]))),
      );
    });
  });
}
