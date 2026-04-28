export class Column {
    _children;
    constructor(children) {
        this._children = children;
    }
    build() {
        const div = document.createElement("div");
        for (let i = 0; i < this._children.length; i++) {
            div.append(this._children[i].build());
        }
        return div;
    }
}
