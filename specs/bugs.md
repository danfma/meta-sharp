# MetaSharp — Bugs (encontrados via SampleIssueTracker)

## Bugs Corrigidos ✅

### B1. `SimpleAssignmentExpression` não suportado
- **Status:** [x] Corrigido — AssignmentExpressionSyntax + MapAssignmentOperator

### B2. Referências estáticas com casing errado
- **Status:** [x] Corrigido — ToCamelCase após type check + enum PascalCase

### B3. Interfaces com métodos → gera interface vazia
- **Status:** [x] Corrigido — TsInterfaceMethod + Printer

### B4. `GenericName` não resolvido em static calls
- **Status:** [x] Corrigido — TransformGenericName()

### B5. `Set<T>` inicializado com `[]`
- **Status:** [x] Corrigido — ConvertedType check + TsArrayLiteral

### B6. Async faltando em overload dispatchers
- **Status:** [x] Corrigido — isAsync = sorted.Any(m => m.IsAsync)

### B7. `ElementAccessExpression` não suportado
- **Status:** [x] Corrigido — TsElementAccess AST node

### F2. `IReadOnlyCollection<T>` mapping
- **Status:** [x] Corrigido — TypeMapper.IsCollectionLike

### F3. `Task.FromResult` / `Task.CompletedTask`
- **Status:** [x] Corrigido — BclMapper → Promise.resolve()

### F4. `DateTimeOffset.UtcNow`
- **Status:** [x] Corrigido — BclMapper → Temporal.Now.zonedDateTimeISO()

---

## Bugs Pendentes (TS build errors do SampleIssueTracker)

### B10. Imports faltando em interfaces — CRÍTICO
- **Sintoma:** `IIssueRepository.ts` usa `IssueId`, `Issue`, `IssueStatus`, `PageRequest`, `PageResult` sem importá-los
- **Erro TS:** `TS2304: Cannot find name 'IssueId'` (e todos os tipos usados nos method signatures)
- **Causa:** `TypeTransformer.TransformInterface()` não coleta imports dos tipos usados nos parâmetros e return types dos métodos da interface. O `CollectImports()` provavelmente não percorre `TsInterfaceMethod`.
- **Fix:** Em `CollectImports()` (TypeTransformer.cs), iterar sobre `TsInterface.Methods` coletando tipos de parâmetros e return types. Mesma lógica já usada para `TsMethodMember`.
- **Status:** [ ] Pendente

### B11. `import type` usado para tipos que são usados como valor (instanceof) — CRÍTICO
- **Sintoma:** `import type { UserId }` mas depois `args[0] instanceof UserId` — TS error
- **Erro TS:** `TS1361: 'UserId' cannot be used as a value because it was imported using 'import type'`
- **Arquivos:** Issue.ts (UserId em overload dispatchers), IssueService.ts
- **Causa:** O transpiler já tem lógica para detectar `new` expressions e importar como valor, mas não detecta `instanceof` checks gerados pelo overload dispatcher. O dispatcher usa `instanceof ClassName` para type guards, mas o import é gerado como type-only.
- **Fix:** Em `CollectImports()`, além de `TsNewExpression`, detectar `TsBinaryExpression` com operador `instanceof` e marcar o tipo como value import. Alternativamente, coletar os tipos usados no dispatcher body.
- **Status:** [ ] Pendente

### B12. `StringEnum` importado como `type` mas usado como valor — CRÍTICO
- **Sintoma:** `import type { IssueStatus }` mas depois `IssueStatus.Backlog` — TS error
- **Erro TS:** `TS2693: 'IssueStatus' only refers to a type, but is being used as a value here`
- **Arquivos:** Issue.ts, IssueWorkflow.ts, IssueQueries.ts
- **Causa:** StringEnums são gerados como `export type IssueStatus = "backlog" | "ready" | ...` (type alias), que realmente são apenas tipos. Mas o transpiler gera código que referencia `IssueStatus.Backlog` como se fosse um namespace/enum com membros. Em TS, string union types não têm membros acessíveis.
- **Fix:** Duas opções:
  1. **Gerar const enum/object:** Em vez de `type IssueStatus = "backlog" | ...`, gerar um `const IssueStatus = { Backlog: "backlog", ... } as const` + `type IssueStatus = typeof IssueStatus[keyof typeof IssueStatus]`. Isso permite usar `IssueStatus.Backlog` como valor.
  2. **Inline os valores:** Quando o transpiler encontra `IssueStatus.Backlog`, gerar o literal `"backlog"` diretamente em vez de `IssueStatus.Backlog`.
  - Opção 1 é melhor pois preserva a semântica e permite autocomplete no IDE.
- **Status:** [ ] Pendente

### B13. Parâmetros de constructor não viram properties acessíveis — CRÍTICO
- **Sintoma:** `constructor(readonly id, title, description, readonly type, priority)` — apenas `id` e `type` são `readonly` (acessíveis). `title`, `description`, `priority` não são acessíveis via `this.title`.
- **Erro TS:** `TS2339: Property 'priority' does not exist on type 'Issue'`
- **Arquivos:** Issue.ts (priority, title, description), Sprint.ts (name, startDate, endDate)
- **Causa:** No C#, primary constructor params com `{ get; }` ou `{ get; private set; }` viram properties. O transpiler marca alguns como `readonly` mas outros ficam como parâmetros simples do constructor (que não criam properties em TS).
- **Fix:** Detectar quais constructor params são properties no C# (têm getter). Para esses, gerar o parâmetro com a visibilidade correta no constructor TS:
  - `{ get; }` → `readonly paramName`
  - `{ get; private set; }` → `paramName` (sem readonly, mas acessível)
  - Parâmetros que não são properties → continuar como params simples
- **Status:** [ ] Pendente

### B14. Static methods referenciam type parameter da classe — MÉDIO
- **Sintoma:** `OperationResult<T>` tem `static ok(value: T)` que referencia `T` da classe
- **Erro TS:** `TS2302: Static members cannot reference class type parameters`
- **Arquivos:** OperationResult.ts
- **Causa:** Em C#, static methods em classes genéricas podem usar `T` porque são `OperationResult<T>.Ok(value)` — o `T` vem do caller. Em TS, static methods não têm acesso ao type param da classe.
- **Fix:** Gerar o static method com seu próprio type parameter: `static ok<T>(value: T): OperationResult<T>`. Detectar quando um static method referencia type params da classe e promovê-los para o método.
- **Status:** [ ] Pendente

### B15. Async overload dispatcher retorna `any` em vez de `Promise<T>` — MÉDIO
- **Sintoma:** `async createAsync(...args: unknown[]): any {` — return type é `any`
- **Erro TS:** `TS1064: The return type of an async function or method must be the global Promise<T> type`
- **Arquivos:** IssueService.ts (createAsync, addCommentAsync)
- **Causa:** Quando os overloads têm return types diferentes mas todos são `Promise<X>`, o `commonReturn` resolve para `any`/`unknown` em vez de `Promise<X>`. O dispatcher deveria ter `Promise<any>` como return type mínimo para async methods.
- **Fix:** Em `GenerateMethodOverloadDispatcher`, quando `isAsync` é true e o commonReturn não é um Promise type, wrappá-lo em `Promise<>`. Se os return types diferem, usar `Promise<any>` ou `Promise<unknown>`.
- **Status:** [ ] Pendente

### B8. `Set.add()` / `Set.delete()` retorno em métodos void — BAIXO
- **Sintoma:** `plan()` gera `return this._plannedIssues.add(issueId)` — retorna Set em vez de void
- **Erro TS:** `TS2322: Type 'Set<IssueId>' is not assignable to type 'void'`
- **Arquivos:** Sprint.ts (plan, unplan), Sprint.ts (rename)
- **Causa:** Expression-bodied methods (`=>`) no C# geram `return expr` no TS, mas quando o return type é void, o `return` não deveria estar lá.
- **Fix:** Em `TransformClassMethod` ou no body transformer, quando `method.ReturnsVoid` é true e o body é expression-bodied, gerar `TsExpressionStatement` em vez de `TsReturnStatement`.
- **Status:** [ ] Pendente

### B9. `Set<T>` getter retorna como `T[]` — BAIXO
- **Sintoma:** `plannedIssues` getter tipo `IssueId[]` mas campo é `Set<IssueId>`
- **Erro TS:** `TS2740: Type 'Set<IssueId>' is missing properties from type 'IssueId[]'`
- **Causa:** `IReadOnlyCollection<T>` mapeia para `T[]` mas o backing field é `Set`
- **Fix:** Detectar quando o return type `IReadOnlyCollection<T>` se refere a um campo `Set<T>` e mapear para `ReadonlySet<T>` ou `Set<T>`. Alternativamente, gerar `[...this._plannedIssues]` no getter body para converter.
- **Status:** [ ] Pendente

### B16. `Guid.NewGuid()` não tem BCL mapping — BAIXO
- **Sintoma:** `Guid.newGuid().toString("N")` — `Guid` não existe no TS
- **Erro TS:** `TS2304: Cannot find name 'Guid'`
- **Arquivos:** IssueId.ts, UserId.ts
- **Causa:** `Guid.NewGuid()` não tem mapping no BclMapper. B2 fix corrigiu o casing (agora é `Guid.NewGuid()`), mas Guid não existe em JS.
- **Fix:** BclMapper: `Guid.NewGuid()` → `crypto.randomUUID()`. O `.ToString("N")` (sem hifens) pode ser mapeado para `.replace(/-/g, "")`.
- **Status:** [ ] Pendente

### B17. `IGrouping<K,V>` não resolvido — BAIXO
- **Sintoma:** `IGrouping<IssueStatus, Issue>` usado em lambda mas tipo não importado/definido
- **Erro TS:** `TS2304: Cannot find name 'IGrouping'`
- **Arquivos:** IssueQueries.ts
- **Causa:** O tipo `Grouping` existe no runtime (`@meta-sharp/runtime`) mas o transpiler gera `IGrouping` (nome C#) em vez de `Grouping` (nome TS). Além disso, não gera o import.
- **Fix:** TypeMapper: `System.Linq.IGrouping<K,V>` → `Grouping<K,V>` (do `@meta-sharp/runtime`). Adicionar import automático.
- **Status:** [ ] Pendente

### B18. Constructor com DI (parâmetro sem property) — BAIXO
- **Sintoma:** `IssueService` constructor recebe `IIssueRepository repository` como DI, gera `private readonly _repository = repository` mas `repository` não existe no escopo
- **Erro TS:** `TS2304: Cannot find name 'repository'` e `TS2304: Cannot find name 'IIssueRepository'`
- **Arquivos:** IssueService.ts
- **Causa:** Em C#, primary constructors capturam parâmetros que podem ser usados em field initializers. No TS, parâmetros de constructor não estão disponíveis em field initializers fora do body.
- **Fix:** Detectar field initializers que referenciam constructor params e mover a atribuição para dentro do constructor body: `constructor(repository: IIssueRepository) { this._repository = repository; }`.
- **Status:** [ ] Pendente

---

## Features Faltantes

### F1. `yield return` / `yield break` → TypeScript generators
- **Impacto:** Métodos que retornam `IEnumerable<T>` com yield
- **Requer:** AST nodes (TsYieldExpression), flag isGenerator, mapeamento IEnumerable→Generator
- **Status:** [ ] Pendente
