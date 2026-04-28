import 'dart:developer' as developer;

/// Routes a generic message to stdout. Mirrors `console.log` in JS (and the
/// BCL `Console.WriteLine`) used by Metano-generated code. Stays on `print`
/// so users see plain output during development; [warn] and [error] use
/// `dart:developer` so they show up correctly in DevTools / structured logs.
void log(Object? object) {
  print(object);
}

/// Routes a warning. Uses `dart:developer` so the output lights up correctly
/// in Flutter DevTools / `dart run` without depending on `dart:io` (which is
/// unavailable on the web target).
void warn(Object? object) {
  developer.log('$object', name: 'metano_runtime', level: 900);
}

/// Routes an error. Same rationale as [warn] for the channel choice.
void error(Object? object) {
  developer.log('$object', name: 'metano_runtime', level: 1000);
}
