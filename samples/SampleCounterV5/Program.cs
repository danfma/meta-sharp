// Top-level statement form. Metano emits this as module-level
// statements (matches SampleCounterV4) instead of a `Program` class.
// The actual Inferno entry call uses `createElement(CounterApp, ...)`
// which the IR pipeline can't lower yet (#189); the TS adapter at
// `Bindings.ts/program.ts` overrides the stubbed body after Metano
// runs.
_ = 0;
