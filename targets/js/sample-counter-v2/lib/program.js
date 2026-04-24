import { Counter } from "#/models";
import { CounterPresenter } from "#/presenters";
import { CounterView } from "#/views";
const view = new CounterView();
const presenter = new CounterPresenter(view, Counter.zero);
presenter.startApplication("root");
