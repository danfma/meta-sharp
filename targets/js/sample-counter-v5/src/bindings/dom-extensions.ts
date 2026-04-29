export function getOrCreateElementById(document: Document, id: string): HTMLElement {
  return document.getElementById(id) ?? document.createElement("div");
}
