# MetaSharp — Bugs (encontrados via SampleIssueTracker)

## Bugs Corrigidos ✅

- **B1** SimpleAssignmentExpression → AssignmentExpressionSyntax no ExpressionTransformer
- **B2** Static references casing → ToCamelCase após type check + enum PascalCase
- **B3** Interface methods vazias → TsInterfaceMethod + Printer
- **B4** GenericName em static calls → TransformGenericName()
- **B5** Set init com `[]` → ConvertedType check + TsArrayLiteral
- **B6** Async em overload dispatchers → isAsync check
- **B7** ElementAccessExpression → TsElementAccess AST node
- **B8** Expression-bodied void methods → isVoid param no TransformBody
- **B10** Imports faltando em interfaces → CollectFromTopLevel visita TsInterface.Methods
- **B11** `import type` vs `instanceof` → valueNames.Add para instanceof
- **B12** StringEnum como type alias → const object + typeof type alias
- **B12b** StringEnum `import type` → HasStringEnum marca como value import
- **B13** Constructor params sem property → `public` explícito no Printer + `None` para exceptions
- **B14** Static methods em generic classes → promove class type params para method level
- **B15** Async overload dispatcher retorna `any` → `Promise<unknown>` fallback
- **B16** Guid.NewGuid() → crypto.randomUUID() + ToString("N") → .replace(/-/g, "")
- **B17** IGrouping<K,V> → Grouping<K,V> do @meta-sharp/runtime + import automático
- **B18** DI constructor params → GetCapturedConstructorParams + atribuição no body
- **B19** IssuePriority não importado → resolvido via PropertyAccess collection + B12b
- **B20** Nullable properties sem initializer → `= null` default
- **F2** IReadOnlyCollection → TypeMapper.IsCollectionLike
- **F3** Task.FromResult/CompletedTask → Promise.resolve()
- **F4** DateTimeOffset.UtcNow → Temporal.Now.zonedDateTimeISO()
- `default` literal → `null` (não `undefined`)
- Import collection: TsArrowFunction params/body, TsPropertyAccess uppercase root, TsElementAccess, TsArrayLiteral

---

## Bugs Pendentes (3 erros restantes no Sprint.ts)

### B9. Set getter retorna como T[] — BAIXO
- **Sintoma:** `plannedIssues` getter tipo `IssueId[]` mas campo é `Set<IssueId>`
- **Erro:** `TS2740: Type 'Set<IssueId>' is missing properties from type 'IssueId[]'`
- **Arquivo:** Sprint.ts
- **Causa:** `IReadOnlyCollection<T>` mapeia para `T[]` via IsCollectionLike, mas o backing field é `Set<T>`
- **Fix:** Opções:
  1. Detectar no getter body que o retorno é um Set field e mapear return type para `ReadonlySet<T>`
  2. Gerar spread no getter body: `return [...this._plannedIssues]` para converter Set → Array
  3. Mapear `IReadOnlyCollection<T>` para `Iterable<T>` (mais genérico, compatível com Set e Array)
- **Status:** [ ] Pendente

### B21. `DateOnly.DayNumber` sem BCL mapping — BAIXO
- **Sintoma:** `endDate.dayNumber - startDate.dayNumber` — PlainDate não tem `dayNumber`
- **Erro:** `TS2339: Property 'dayNumber' does not exist on type 'PlainDate'`
- **Arquivo:** Sprint.ts (2 ocorrências)
- **Causa:** `DateOnly.DayNumber` retorna dias desde 0001-01-01 (epoch do .NET). Temporal.PlainDate não tem equivalente direto.
- **Fix:** Opções:
  1. Mapear `DateOnly.DayNumber` para uma helper function no runtime: `dayNumber(date: Temporal.PlainDate): number`
  2. Para subtração de datas (`a.DayNumber - b.DayNumber`), detectar o padrão e gerar `a.until(b).days` (Temporal duration)
  3. Adicionar property mapping no BclMapper.TryMap: `DateOnly.DayNumber` → helper ou inline computation
- **Status:** [ ] Pendente

---

## Features Faltantes

### F1. `yield return` / `yield break` → TypeScript generators
- **Status:** [x] Corrigido (Codex — commit 0c938c5)
