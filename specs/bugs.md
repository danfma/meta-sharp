# MetaSharp — Bugs (encontrados via SampleIssueTracker)

## Todos os bugs corrigidos ✅

O SampleIssueTracker compila com **0 erros TypeScript**.

### Bugs do transpiler
- **B1** SimpleAssignmentExpression
- **B2** Static references casing (PascalCase para tipos/enums)
- **B3** Interface methods vazias → TsInterfaceMethod
- **B4** GenericName em static calls
- **B5** Set/HashSet init com `[]` → `new HashSet()`
- **B6** Async em overload dispatchers
- **B7** ElementAccessExpression (`arr[i]`)
- **B8** Expression-bodied void methods (sem `return`)
- **B9** IReadOnlyCollection → `Iterable<T>` (compatível com Array e HashSet)
- **B10** Imports em interfaces (TsInterfaceMethod)
- **B11** `import type` vs `instanceof`
- **B12** StringEnum → const object + typeof type alias
- **B12b** StringEnum import como valor
- **B13** Constructor params → `public` explícito + `None` para exceptions
- **B14** Static methods em generic classes (promove type params)
- **B15** Async overload dispatcher → `Promise<unknown>`
- **B16** Guid.NewGuid() → `crypto.randomUUID()` + ToString("N")
- **B17** IGrouping → Grouping (runtime import)
- **B18** DI constructor params (captured field initializers)
- **B19** IssuePriority import via PropertyAccess collection
- **B20** Nullable properties → `= null` default
- **B21** DateOnly.DayNumber → `dayNumber()` runtime helper

### Features
- **F1** yield return/break → TypeScript generators
- **F2** IReadOnlyCollection mapping
- **F3** Task.FromResult/CompletedTask → Promise.resolve()
- **F4** DateTimeOffset.UtcNow → Temporal.Now.zonedDateTimeISO()

### Melhorias de import collection
- TsArrowFunction params/body
- TsPropertyAccess uppercase root como value
- TsFieldMember types e initializers
- TsElementAccess e TsArrayLiteral

### Runtime
- HashSet<T> com equals/hashCode (system/collections/)
- dayNumber() helper (temporal-helpers)
- LINQ operators (distinct, union, intersect, except) usam HashSet
