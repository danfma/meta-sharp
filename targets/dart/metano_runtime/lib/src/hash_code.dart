import 'dart:typed_data';

/// Port of System.HashCode from .NET (xxHash32-based).
///
/// Mirrors the TypeScript port at `metano-runtime/src/system/hash-code.ts`.
/// Used by Metano-generated record classes and branded types for
/// deterministic, cross-platform value hashing.
///
/// Reference: https://github.com/dotnet/corert/blob/master/src/System.Private.CoreLib/shared/System/HashCode.cs
/// Algorithm: xxHash32 by Yann Collet (BSD 2-Clause)

const int _prime1 = 0x9e3779b1;
const int _prime2 = 0x85ebca77;
const int _prime3 = 0xc2b2ae3d;
const int _prime4 = 0x27d4eb2f;
const int _prime5 = 0x165667b1;

const int _seed = 0;
const int _mask32 = 0xFFFFFFFF;

int _toInt32(int x) => (x & _mask32).toSigned(32);

/// 32-bit multiplication producing the low 32 bits, mirroring `Math.imul` in JS.
/// Splitting into 16-bit halves keeps the intermediate products inside the
/// 53-bit safe-integer range so the result is identical on Dart VM and on
/// dart2js/dart2wasm targets (Flutter Web).
int _imul(int a, int b) {
  final aHi = (a >> 16) & 0xFFFF;
  final aLo = a & 0xFFFF;
  final bHi = (b >> 16) & 0xFFFF;
  final bLo = b & 0xFFFF;
  final lo = aLo * bLo;
  final mid = ((aHi * bLo) + (aLo * bHi)) & _mask32;
  return _toInt32(lo + ((mid << 16) & _mask32));
}

int _rotateLeft(int value, int count) {
  final v = value & _mask32;
  return _toInt32((v << count) | (v >>> (32 - count)));
}

int _round(int hash, int input) {
  hash = _toInt32(hash + _imul(input, _prime2));
  hash = _rotateLeft(hash, 13);
  return _imul(hash, _prime1);
}

int _queueRound(int hash, int queuedValue) {
  hash = _toInt32(hash + _imul(queuedValue, _prime3));
  hash = _rotateLeft(hash, 17);
  return _imul(hash, _prime4);
}

int _mixState(int v1, int v2, int v3, int v4) {
  return _toInt32(
    _rotateLeft(v1, 1) +
        _rotateLeft(v2, 7) +
        _rotateLeft(v3, 12) +
        _rotateLeft(v4, 18),
  );
}

int _mixFinal(int hash) {
  hash = _toInt32(hash ^ ((hash & _mask32) >>> 15));
  hash = _imul(hash, _prime2);
  hash = _toInt32(hash ^ ((hash & _mask32) >>> 13));
  hash = _imul(hash, _prime3);
  hash = _toInt32(hash ^ ((hash & _mask32) >>> 16));
  return hash;
}

int _mixEmptyState() => _toInt32(_seed + _prime5);

bool _fitsInt32(double value) {
  return value == value.truncateToDouble() &&
      value >= -2147483648 &&
      value <= 2147483647;
}

int _hashValue(Object? value) {
  if (value == null) return 0;
  if (value is bool) return value ? 1 : 0;
  if (value is int) return _toInt32(value);
  if (value is double) {
    if (value.isNaN) return 0;
    if (_fitsInt32(value)) return _toInt32(value.toInt());
    final bytes = ByteData(8)..setFloat64(0, value, Endian.little);
    return _toInt32(
      bytes.getInt32(0, Endian.little) ^ bytes.getInt32(4, Endian.little),
    );
  }
  if (value is String) {
    var hash = 0;
    for (var i = 0; i < value.length; i++) {
      hash = _toInt32(_imul(hash, 31) + value.codeUnitAt(i));
    }
    return hash;
  }
  if (value is BigInt) {
    return _toInt32((value & BigInt.from(_mask32)).toInt());
  }
  return _toInt32(value.hashCode);
}

/// Accumulator for computing hash codes, modeled after `System.HashCode`.
///
/// The Metano compiler emits the smallest API that fits the field count:
/// 1 → 4 fields use the static [combine]/[combine2]/[combine3]/[combine4]
/// helpers; records with 5+ fields use the builder form below.
///
/// ```dart
/// final hc = HashCode()
///   ..add(this.x)
///   ..add(this.y)
///   ..add(this.z);
/// return hc.toHashCode();
/// ```
class HashCode {
  HashCode()
    : _v1 = _toInt32(_seed + _prime1 + _prime2),
      _v2 = _toInt32(_seed + _prime2),
      _v3 = _seed,
      _v4 = _toInt32(_seed - _prime1);

  int _v1;
  int _v2;
  int _v3;
  int _v4;
  int _queue1 = 0;
  int _queue2 = 0;
  int _queue3 = 0;
  int _length = 0;

  void add(Object? value) {
    final hc = _hashValue(value);
    final pos = _length % 4;
    _length++;
    switch (pos) {
      case 0:
        _queue1 = hc;
        break;
      case 1:
        _queue2 = hc;
        break;
      case 2:
        _queue3 = hc;
        break;
      case 3:
        _v1 = _round(_v1, _queue1);
        _v2 = _round(_v2, _queue2);
        _v3 = _round(_v3, _queue3);
        _v4 = _round(_v4, hc);
        break;
    }
  }

  int toHashCode() {
    final length = _length;
    var hash = length >= 4 ? _mixState(_v1, _v2, _v3, _v4) : _mixEmptyState();

    hash = _toInt32(hash + length * 4);

    final remaining = length % 4;
    if (remaining >= 1) hash = _queueRound(hash, _queue1);
    if (remaining >= 2) hash = _queueRound(hash, _queue2);
    if (remaining >= 3) hash = _queueRound(hash, _queue3);

    return _mixFinal(hash);
  }

  static int combine(Object? v1) {
    final hc = HashCode()..add(v1);
    return hc.toHashCode();
  }

  static int combine2(Object? v1, Object? v2) {
    final hc = HashCode()
      ..add(v1)
      ..add(v2);
    return hc.toHashCode();
  }

  static int combine3(Object? v1, Object? v2, Object? v3) {
    final hc = HashCode()
      ..add(v1)
      ..add(v2)
      ..add(v3);
    return hc.toHashCode();
  }

  static int combine4(Object? v1, Object? v2, Object? v3, Object? v4) {
    final hc = HashCode()
      ..add(v1)
      ..add(v2)
      ..add(v3)
      ..add(v4);
    return hc.toHashCode();
  }
}
