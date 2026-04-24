export class Renderer {
    _container;
    constructor(containerId) {
        this._container = document.getElementById(containerId) ?? this.createElement(containerId);
    }
    createElement(containerId) {
        const element = document.createElement("div");
        element.id = containerId;
        document.body.append(element);
        return element;
    }
    render(view) {
        view.render(this._container);
    }
}
