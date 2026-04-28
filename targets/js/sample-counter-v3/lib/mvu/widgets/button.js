export class Button {
    _label;
    _onPressed;
    constructor(label, onPressed) {
        this._label = label;
        this._onPressed = onPressed;
    }
    build() {
        const btn = document.createElement("button");
        btn.innerHTML = this._label;
        btn.onclick = (_) => this._onPressed();
        return btn;
    }
}
