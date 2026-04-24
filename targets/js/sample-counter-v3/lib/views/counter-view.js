export class CounterView {
    _text = null;
    onButtonClick = null;
    constructor() { }
    render(container) {
        const root = document.createElement("div");
        container.append(root);
        const text = document.createElement("span");
        text.innerHTML = "0";
        root.append(text);
        this._text = text;
        const button = document.createElement("button");
        button.innerHTML = "Click me";
        button.onclick = (_) => this.onButtonClick?.();
        root.append(button);
    }
    showCounter(counter) {
        this._text != null && (this._text.innerHTML = counter.toString());
    }
}
