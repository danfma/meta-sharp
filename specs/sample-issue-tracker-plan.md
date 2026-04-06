# Sample Plan — `SampleIssueTracker`

## Objetivo

Criar uma sample mais realista que `SampleTodo`, com arquitetura limpa e regras de domínio suficientes para validar a geração do MetaSharp sem introduzir banco, HTTP ou DI.

## Fora de Escopo

- Não mexer no runtime, CLI, `.metalib`, config file ou watch mode
- Não adicionar integrações externas, persistência real ou framework web
- Não usar a sample para empurrar features novas sem necessidade clara de validação

## Estrutura Proposta

```text
SampleIssueTracker/
├── AssemblyInfo.cs
├── Issues/
│   ├── Domain/
│   │   ├── IssueId.cs
│   │   ├── IssueStatus.cs
│   │   ├── IssuePriority.cs
│   │   ├── IssueType.cs
│   │   ├── Comment.cs
│   │   ├── Issue.cs
│   │   └── IssueWorkflow.cs
│   └── Application/
│       ├── IIssueRepository.cs
│       ├── InMemoryIssueRepository.cs
│       ├── IssueService.cs
│       └── IssueQueries.cs
├── Planning/
│   └── Domain/
│       └── Sprint.cs
└── SharedKernel/
    ├── OperationResult.cs
    ├── PageRequest.cs
    ├── PageResult.cs
    └── UserId.cs
```

Saída gerada:

```text
js/sample-issue-tracker/
├── src/
├── dist/
└── package.json
```

## Cobertura de Features por Arquivo

- `IssueStatus`, `IssuePriority`, `IssueType`: `[StringEnum]`, `[Name]`, unions TS e guards
- `OperationResult<T>`, `PageResult<T>`: records genéricos, imports de type parameters e `with()`
- `IssueId`, `UserId`: value objects simples, `equals()`, `hashCode()`
- `Comment`, `Sprint`: datas, coleções e nullable em um domínio mais realista
- `Issue`: composição rica, propriedades calculadas, overloads e métodos de domínio
- `IssueWorkflow`: switch/pattern matching, regras de transição e overloads utilitários
- `IIssueRepository`: interfaces genéricas/async com `Task<T>` e nullables
- `InMemoryIssueRepository`: LINQ, `Map/Set/Array`, async e shape estável para transpilar
- `IssueService`: orquestração de aplicação, overloads simples, `OperationResult<T>`
- `IssueQueries`: `[ExportedAsModule]` para funções top-level e consulta funcional

## Casos que a Sample Deve Validar

- Transição de status válida e inválida (`Backlog -> InProgress -> Done`)
- Filtros por sprint, prioridade, assignee e status
- Paginação simples com `PageRequest` e `PageResult<T>`
- Comentários e histórico leve com timestamps
- Queries com LINQ (`Where`, `Select`, `GroupBy`, `OrderBy`, `Any`, `Count`)
- Guards gerados para tipos centrais (`Issue`, `Comment`, `Sprint`)
- Barrel exports e imports entre pastas e arquivos
- Saída JS legível o bastante para servir como documentação viva

## Ordem de Implementação

1. Shared kernel e tipos base: IDs compartilhados, paginação, resultado e `AssemblyInfo.cs`
2. Domínio de issues e planejamento: `Issue`, `Comment`, `Sprint`, `IssueWorkflow`
3. Aplicação de issues: repositório em memória, service, queries exportadas como módulo
4. Geração TS: novo `js/sample-issue-tracker`, build com `tsgo`, testes Bun
5. Verificação: snapshots dos arquivos gerados mais importantes e smoke tests end-to-end

## Definição de Pronto

- A solução C# compila e transpila sem ajustes manuais no output
- O pacote `js/sample-issue-tracker` builda e testa com Bun
- A sample demonstra claramente domínio, aplicação e shared kernel separados por linguagem de negócio
- O código gerado cobre enums, generics, async, LINQ, guards e módulo exportado

## Critérios de Qualidade

- A sample deve permanecer sem infraestrutura externa
- O domínio deve parecer código real, não um “feature zoo”
- Cada arquivo novo precisa justificar pelo menos uma capacidade importante do transpiler
- O output gerado em TypeScript deve ser estável o bastante para revisão humana
