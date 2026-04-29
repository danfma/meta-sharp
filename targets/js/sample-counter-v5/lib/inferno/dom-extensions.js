export function getOrCreateElementById(document, id) {
    return document.getElementById(id) ?? document.createElement("div");
}
