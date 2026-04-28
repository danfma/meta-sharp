# Plano de Suporte Aprimorado para Flutter/Dart

Este plano define a estratégia para elevar o suporte ao Flutter/Dart no compilador Metano, alinhando-o com a maturidade do suporte TypeScript.

## 1. Suposições
- O compilador Dart opera de forma análoga ao TypeScript (IR -> Dart AST -> Printer).
- O `metano_runtime` (package Dart criado) será a "âncora" para todas as dependências geradas pelo compilador.
- A herança de `MetanoObject` (ou interfaces equivalentes) será a base para garantir compatibilidade com o runtime.
- O `Printer.cs` atual precisa de capacidade para emitir diretivas `import` no topo dos arquivos gerados.

## 2. Direções de Implementação

### A. Integração de Runtime
- **Injeção de Imports:** Modificar o `Printer.cs` para suportar a emissão de cabeçalhos de importação.
- **Coletor de Imports:** Criar um `DartImportCollector` (similar ao TypeScript) para identificar dependências (ex: `metano_runtime`, coleções) e evitar duplicação.
- **Herança Base:** Ajustar `IrToDartClassBridge.cs` para que classes geradas estendam `MetanoObject` por padrão, caso não tenham uma base definida.

### B. Orquestração de Build
- **Sincronização:** Implementar lógica no `DartTarget` para identificar o ambiente Dart (usando `pubspec.yaml` e pacotes locais) durante a transpilação.
- **Integração `build_runner`:** Explorar a viabilidade de usar `build_runner` no diretório de targets para que o desenvolvedor Flutter não precise rodar o compilador manualmente.

### C. Refinamento de Tipos (BCL)
- **Adaptadores:** Mapear classes comuns do C# (ex: `Console`, coleções) para as abstrações do `metano_runtime`.
- **Interface Mappings:** Garantir que interfaces do C# que não têm equivalentes diretos no Dart sejam filtradas ou mapeadas adequadamente, evitando `UnimplementedError` em tempo de compilação.

## 3. Fluxo de Validação
1. **Fase 1 (Injeção):** Validar a injeção do import e a herança de `MetanoObject` no código gerado (`Counter.dart`).
2. **Fase 2 (Compilação):** Rodar `flutter pub get` e `flutter build` no sample para garantir que o código gerado compilou com sucesso após as mudanças.
3. **Fase 3 (Runtime):** Garantir que o app contador continue funcionando com a nova herança base.

---
*Status: Aguardando validação do plano.*
