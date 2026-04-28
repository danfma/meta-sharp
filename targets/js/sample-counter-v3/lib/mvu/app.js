import { StateHolder } from "./state-holder";
export class App {
    constructor() { }
    static mount(containerId, initialState, view) {
        const container = App.resolveContainer(containerId);
        const holder = new StateHolder(initialState);
        App.apply(view(holder.state, holder.set.bind(holder)), container);
        holder.onChange = () => App.apply(view(holder.state, holder.set.bind(holder)), container);
    }
    static apply(root, container) {
        container.innerHTML = "";
        container.append(root.build());
    }
    static resolveContainer(containerId) {
        const existing = document.getElementById(containerId);
        if (existing != null) {
            return existing;
        }
        const element = document.createElement("div");
        element.id = containerId;
        document.body.append(element);
        return element;
    }
}
