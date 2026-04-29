import { Button, Column as ColumnWidget, Heading, Row, Text } from "#/mvu/widgets";
import type { Widget } from "./widget";

export function Column(args: { gap: number; children: Widget[] }): ColumnWidget {
  const { gap, children } = args;

  return new ColumnWidget(gap, children);
}

export function row(args: { gap: number; children: Widget[] }): Row {
  const { gap, children } = args;

  return new Row(gap, children);
}

export function text(args: { content: string }): Text {
  const { content } = args;

  return new Text(content);
}

export function heading(args: { content: string; level?: number }): Heading {
  const { content, level = 1 } = args;

  return new Heading(content, level);
}

export function button(args: { label: string; onPressed: () => void }): Button {
  const { label, onPressed } = args;

  return new Button(label, onPressed);
}
