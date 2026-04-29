import { HashCode } from "metano-runtime";
export class Counter {
    count;
    constructor(count) {
        this.count = count;
    }
    static get zero() {
        return new Counter(0);
    }
    increment() {
        return new Counter(this.count + 1);
    }
    decrement() {
        return new Counter(this.count - 1);
    }
    equals(other) {
        return other instanceof Counter && this.count === other.count;
    }
    hashCode() {
        const hc = new HashCode();
        hc.add(this.count);
        return hc.toHashCode();
    }
    with(overrides) {
        return new Counter(overrides?.count ?? this.count);
    }
}
