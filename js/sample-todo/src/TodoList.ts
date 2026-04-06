import { isString } from "@meta-sharp/runtime";
import { Enumerable } from "@meta-sharp/runtime";
import type { Priority } from "./Priority";
import { TodoItem } from "./TodoItem";
export class TodoList {
  constructor(readonly name: string) { }

  private readonly _items: TodoItem[] = [];

  get items(): TodoItem[] {
    return this._items;
  }

  get count(): number {
    return this._items.length;
  }

  get pendingCount(): number {
    return Enumerable.from(this._items).count((i: TodoItem) => !i.completed);
  }

  add(title: string, priority: Priority): void;
  add(item: TodoItem): void;
  add(title: string): void;
  add(...args: unknown[]): void {
    if (args.length === 2 && isString(args[0]) && (args[1] === "low" || args[1] === "medium" || args[1] === "high")) {
      const title = args[0] as string;
      const priority = args[1] as Priority;
      this._items.push(new TodoItem(title, false, priority));
      return;
    }
    if (args.length === 1 && args[0] instanceof TodoItem) {
      const item = args[0] as TodoItem;
      this._items.push(item);
      return;
    }
    if (args.length === 1 && isString(args[0])) {
      const title = args[0] as string;
      this._items.push(new TodoItem(title));
      return;
    }
    throw new Error("No matching overload for add");
  }

  findByTitle(title: string): TodoItem | null {
    return Enumerable.from(this._items).firstOrDefault((i: TodoItem) => i.title === title);
  }

  hasPending(): boolean {
    return Enumerable.from(this._items).any((i: TodoItem) => !i.completed);
  }
}
