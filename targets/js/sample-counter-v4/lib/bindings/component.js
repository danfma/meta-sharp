// Component class re-export so `class CounterApp extends Component<...>`
// resolves at runtime. Metano emits a stub class for this declaration
// today; we override with the real npm export.
export { Component } from "inferno";
