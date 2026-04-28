export class Text {
    _content;
    constructor(content) {
        this._content = content;
    }
    build() {
        const span = document.createElement("span");
        span.innerHTML = this._content;
        return span;
    }
}
