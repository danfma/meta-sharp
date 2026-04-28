import { Button, Column, Heading, Row, Text } from "#/mvu/widgets";
import type { Widget } from "./widget";

export class UI {
  constructor() { }

  static column(gap: number, children: Widget[]): Column {
    return new Column(gap, children);
  }

  static row(gap: number, children: Widget[]): Row {
    return new Row(gap, children);
  }

  static text(content: string): Text {
    return new Text(content);
  }

  static heading(content: string, level: number = 1): Heading {
    return new Heading(content, level);
  }

  static button(label: string, onPressed: () => void): Button {
    return new Button(label, onPressed);
  }
}
