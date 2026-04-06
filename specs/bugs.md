# MetaSharp — Bugs (encontrados via SampleIssueTracker)

## Bugs Corrigidos ✅

- **B1** SimpleAssignmentExpression → AssignmentExpressionSyntax no ExpressionTransformer
- **B2** Static references casing → ToCamelCase após type check + enum PascalCase
- **B3** Interface methods vazias → TsInterfaceMethod + Printer
- **B4** GenericName em static calls → TransformGenericName()
- **B5** Set init com `[]` → ConvertedType check + TsArrayLiteral
- **B6** Async em overload dispatchers → isAsync check
- **B7** ElementAccessExpression → TsElementAccess AST node
- **B10** Imports faltando em interfaces → CollectFromTopLevel visita TsInterface.Methods
- **B11** `import type` vs `instanceof` → valueNames.Add para instanceof
- **B12** StringEnum como type alias → const object + typeof type alias
- **B13** Constructor params sem property → `public` explícito no Printer + `None` para exceptions
- **F2** IReadOnlyCollection → TypeMapper.IsCollectionLike
- **F3** Task.FromResult/CompletedTask → BclMapper Promise.resolve()
- **F4** DateTimeOffset.UtcNow → BclMapper Temporal.Now.zonedDateTimeISO()

---

## Bugs Pendentes (TS build errors restantes)

### B12b. StringEnum `import type` em arquivos que usam como valor — CRÍTICO
- **Sintoma:** `import type { IssueStatus }` mas `IssueStatus.Backlog` usado como valor
- **Erro:** `TS1361: 'IssueStatus' cannot be used as a value because it was imported using 'import type'`
- **Arquivos:** Issue.ts, IssueWorkflow.ts, IssueQueries.ts, IssueService.ts
- **Causa:** StringEnums agora são const objects (valor + tipo), mas `CollectImports` ainda os importa como type-only. O transpiler precisa detectar que StringEnums com const object são values.
- **Fix:** Em `CollectImports`, quando um tipo referenciado é um StringEnum (tem `[StringEnum]`), adicioná-lo a `valueNames` para gerar `import { X }` em vez de `import type { X }`. Detectar via `SymbolHelper.HasStringEnum()` no loop de resolução.
- **Status:** [ ] Pendente

### B14. Static methods referenciam type parameter da classe — MÉDIO
- **Sintoma:** `static ok(value: T): OperationResult<T>` referencia `T` da classe
- **Erro:** `TS2302: Static members cannot reference class type parameters`
- **Arquivo:** OperationResult.ts
- **Fix:** Detectar quando static method usa type params da classe e promovê-los como type params do método: `static ok<T>(value: T): OperationResult<T>`.
- **Status:** [ ] Pendente

### B15. Async overload dispatcher retorna `any` — MÉDIO
- **Sintoma:** `async createAsync(...args: unknown[]): any {`
- **Erro:** `TS1064: The return type of an async function or method must be the global Promise<T> type`
- **Arquivo:** IssueService.ts
- **Fix:** Quando `isAsync` e commonReturn não é Promise, usar `Promise<unknown>` como tipo do dispatcher.
- **Status:** [ ] Pendente

### B18. Constructor com DI (primary ctor param em field initializer) — MÉDIO
- **Sintoma:** `private readonly _repository = repository` — `repository` não está em escopo
- **Erro:** `TS2304: Cannot find name 'repository'` + `TS2304: Cannot find name 'IIssueRepository'`
- **Arquivo:** IssueService.ts
- **Causa:** Primary constructor params em C# podem ser usados em field initializers. Em TS, field initializers não têm acesso a params do constructor.
- **Fix:** Detectar fields que referenciam constructor params e mover a atribuição para dentro do constructor body.
- **Status:** [ ] Pendente

### B17. `IGrouping<K,V>` não resolvido — MÉDIO
- **Sintoma:** `IGrouping<IssueStatus, Issue>` não importado/definido
- **Erro:** `TS2304: Cannot find name 'IGrouping'`
- **Arquivo:** IssueQueries.ts
- **Fix:** TypeMapper: `System.Linq.IGrouping<K,V>` → `Grouping<K,V>` (de `@meta-sharp/runtime`). Gerar import automático.
- **Status:** [ ] Pendente

### B19. IssuePriority não importado em IssueWorkflow.ts — MÉDIO
- **Sintoma:** `IssuePriority.Urgent` usado mas tipo não importado
- **Erro:** `TS2304: Cannot find name 'IssuePriority'`
- **Arquivo:** IssueWorkflow.ts, IssueQueries.ts
- **Causa:** Mesmo que B12b — StringEnum importado como type ou não importado
- **Status:** [ ] Pendente (resolvido junto com B12b)

### B8. Expression-bodied void methods geram `return` — BAIXO
- **Sintoma:** `plan()` gera `return this._plannedIssues.add(issueId)`
- **Erro:** `TS2322: Type 'Set<IssueId>' is not assignable to type 'void'`
- **Arquivo:** Sprint.ts (plan, unplan, rename)
- **Fix:** Em body transformer, quando `method.ReturnsVoid` e body é expression-bodied, gerar `TsExpressionStatement` em vez de `TsReturnStatement`.
- **Status:** [ ] Pendente

### B9. Set getter retorna como T[] — BAIXO
- **Sintoma:** `plannedIssues` getter tipo `IssueId[]` mas campo é `Set<IssueId>`
- **Erro:** `TS2740: Type 'Set<IssueId>' is missing properties from type 'IssueId[]'`
- **Arquivo:** Sprint.ts
- **Fix:** Detectar quando return type é Set e mapear para `ReadonlySet<T>` ou gerar spread `[...this._field]`.
- **Status:** [ ] Pendente

### B16. `Guid.NewGuid()` sem BCL mapping — BAIXO
- **Sintoma:** `Guid.newGuid().toString("N")` — `Guid` não existe
- **Erro:** `TS2304: Cannot find name 'Guid'`
- **Arquivo:** IssueId.ts, UserId.ts
- **Fix:** BclMapper: `Guid.NewGuid()` → `crypto.randomUUID()`. `.ToString("N")` → `.replace(/-/g, "")`.
- **Status:** [ ] Pendente

### B20. Nullable properties sem initializer no constructor — BAIXO
- **Sintoma:** `assigneeId: UserId | null;` sem initializer
- **Erro:** `TS2564: Property has no initializer and is not definitely assigned in the constructor`
- **Arquivo:** Issue.ts
- **Fix:** Gerar initializer `= null` para nullable properties que não recebem valor no constructor.
- **Status:** [ ] Pendente

---

## Features Faltantes

### F1. `yield return` / `yield break` → TypeScript generators
- **Status:** [ ] Pendente
