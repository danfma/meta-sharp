# Same-namespace Relative Imports Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** simplificar a exceção de imports no mesmo namespace, trocando o fallback atual via alias do package por imports relativos locais.

**Architecture:** a estratégia namespace-first continua como padrão do transpiler, mas o caso “mesmo namespace” passa a ser tratado como uma regra explícita de import relativo entre arquivos irmãos. Isso reduz ruído no output, evita passar pelo barrel do próprio namespace e mantém o ciclo artificial fora do caminho.

**Tech Stack:** C#, Roslyn, transpiler TypeScript do Metano, NUnit, sample packages em `js/`.

---

## Regra alvo

- mesmo namespace:
  - `import { IssueWorkflow } from "./issue-workflow"`
  - `import type { IssueId } from "./issue-id"`
- namespace diferente no mesmo package:
  - `import { Issue } from "#/issues/domain"`
- cross-package:
  - `import { Money } from "@scope/lib/domain"`
- root namespace do assembly:
  - `import { Widget } from "@scope/lib"`

---

## Task 1: Ajustar a resolução de paths locais

**Files:**
- Modify: `src/Metano.Compiler.TypeScript/Transformation/PathNaming.cs`
- Modify: `src/Metano.Compiler.TypeScript/Transformation/ImportCollector.cs`
- Test: `tests/Metano.Tests/NamespaceTranspileTests.cs`
- Test: `tests/Metano.Tests/InheritanceTranspileTests.cs`

**Step 1: Criar helper de import relativo por arquivo**

Adicionar um helper dedicado para o caso:

- origem e destino no mesmo namespace lógico
- destino diferente do arquivo atual

Saída esperada:

- `./currency`
- `./issue-workflow`

**Step 2: Preservar imports barrel-first para namespaces diferentes**

Não mexer no comportamento já validado para:

- `#`
- `#/foo/bar`
- `@scope/pkg`
- `@scope/pkg/foo/bar`

**Step 3: Atualizar comentários e nomes de API**

Deixar claro no código que:

- same namespace = relative file import
- different namespace = namespace barrel

---

## Task 2: Atualizar testes do compilador

**Files:**
- Modify: `tests/Metano.Tests/NamespaceTranspileTests.cs`
- Modify: `tests/Metano.Tests/EmitInFileTests.cs`
- Modify: `tests/Metano.Tests/Expected/*.ts` quando necessário

**Step 1: Cobrir mesmo namespace com import relativo**

Exemplos:

- `Money` importa `Currency` como `./currency`
- `Issue` importa `IssueWorkflow` como `./issue-workflow`

**Step 2: Cobrir `[EmitInFile]`**

Quando um grouped file importa outro arquivo do mesmo namespace:

- usar `./tag`
- não `#/tag`

**Step 3: Garantir que cross-namespace permanece barrel-first**

Manter asserts como:

- `from "#"`
- `from "#/issues/domain"`

---

## Task 3: Verificar impacto no detector de ciclos

**Files:**
- Modify: `src/Metano.Compiler.TypeScript/Transformation/CyclicReferenceDetector.cs`
- Test: `tests/Metano.Tests/CyclicReferenceTests.cs`

**Step 1: Confirmar que imports relativos não entram no grafo de barrels**

O detector deve continuar focado no contrato de alias do package.

**Step 2: Garantir que warnings úteis continuam existindo**

Se um ciclo real persistir via barrels entre namespaces diferentes, o warning ainda
deve aparecer.

---

## Task 4: Regenerar a amostra e validar o output

**Files:**
- Modify: `js/sample-issue-tracker/src/**/*.ts`
- Modify: `js/sample-issue-tracker/test/**/*.ts`
- Optional: `js/sample-issue-tracker/tsconfig.json`

**Step 1: Regenerar**

Rodar:

- `bun run generate`

**Step 2: Inspecionar o namespace `issues/domain`**

Esperado:

- `issue.ts` usa `./issue-workflow`
- `issue-workflow.ts` usa `./issue`
- `comment.ts` usa relativo para irmãos quando aplicável

**Step 3: Confirmar que `issues/application` segue barrel-first**

Esperado:

- `IssueService` continua importando de `#/issues/domain`
- `shared-kernel` continua vindo de `#/shared-kernel`

---

## Verificação

Antes de considerar concluído:

- `dotnet msbuild tests/Metano.Tests/Metano.Tests.csproj /t:Test`
- `bun run generate` em `js/sample-issue-tracker`
- inspeção visual dos imports gerados no mesmo namespace
