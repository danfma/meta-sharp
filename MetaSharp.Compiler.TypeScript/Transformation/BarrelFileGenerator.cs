using MetaSharp.TypeScript.AST;

namespace MetaSharp.Transformation;

/// <summary>
/// Generates leaf-only `index.ts` barrel files for the TypeScript output.
///
/// One barrel is emitted per directory that contains generated type files; sub-directories
/// are NOT re-exported (consumers must use full subpath imports such as
/// `import { Issue } from "package/issues/domain"`). When a directory already contains a
/// user-defined type whose file is named `index.ts`, the barrel is skipped to avoid a
/// collision.
///
/// Pure / stateless: takes the list of generated <see cref="TsSourceFile"/>s and returns
/// the index files to append to it.
/// </summary>
public static class BarrelFileGenerator
{
    public static IReadOnlyList<TsSourceFile> Generate(IReadOnlyList<TsSourceFile> typeFiles)
    {
        // Group files by their directory
        var dirToFiles = new Dictionary<string, List<TsSourceFile>>();

        foreach (var file in typeFiles)
        {
            var dir = Path.GetDirectoryName(file.FileName)?.Replace('\\', '/') ?? "";
            if (!dirToFiles.TryGetValue(dir, out var list))
            {
                list = [];
                dirToFiles[dir] = list;
            }

            list.Add(file);
        }

        var indexFiles = new List<TsSourceFile>();

        foreach (var (dir, files) in dirToFiles)
        {
            var exports = new List<TsTopLevel>();

            foreach (var file in files.OrderBy(f => f.FileName))
            {
                var moduleName = Path.GetFileNameWithoutExtension(file.FileName);

                // Collect all exported names from this file. If a name has BOTH a value and a
                // type form (e.g., StringEnum: const + type alias, InlineWrapper: namespace + type),
                // re-export as value (declaration merging on the import side).
                var valueNames = new HashSet<string>(StringComparer.Ordinal);
                var typeOnlyNames = new HashSet<string>(StringComparer.Ordinal);

                foreach (var stmt in file.Statements)
                {
                    var name = GetExportedName(stmt);
                    if (name is null) continue;

                    if (IsTypeOnlyExport(stmt))
                        typeOnlyNames.Add(name);
                    else
                        valueNames.Add(name);
                }

                // A name that's both a value and a type → only emit as value (the import
                // pulls both via TS declaration merging).
                typeOnlyNames.ExceptWith(valueNames);

                if (valueNames.Count > 0)
                    exports.Add(new TsReExport([.. valueNames.OrderBy(n => n)], $"./{moduleName}"));

                if (typeOnlyNames.Count > 0)
                    exports.Add(new TsReExport([.. typeOnlyNames.OrderBy(n => n)], $"./{moduleName}", TypeOnly: true));
            }

            // Leaf-only barrels: do NOT re-export subdirectories. Consumers must use full
            // paths (e.g., `import { Issue } from "package/issues/domain/issue"`) or import
            // from the leaf barrel (`from "package/issues/domain"`).

            // Skip barrel generation if a user-defined type would collide with the barrel
            // file name (e.g., a type named "Index" produces "index.ts" already).
            var hasIndexCollision = files.Any(f =>
                Path.GetFileName(f.FileName).Equals("index.ts", StringComparison.OrdinalIgnoreCase));
            if (hasIndexCollision)
                continue;

            if (exports.Count > 0)
            {
                var indexPath = dir.Length > 0 ? $"{dir}/index.ts" : "index.ts";
                indexFiles.Add(new TsSourceFile(indexPath, exports, ""));
            }
        }

        return indexFiles;
    }

    private static string? GetExportedName(TsTopLevel node) => node switch
    {
        TsClass { Exported: true } c => c.Name,
        TsFunction { Exported: true } f => f.Name,
        TsEnum { Exported: true } e => e.Name,
        TsTypeAlias { Exported: true } t => t.Name,
        TsInterface { Exported: true } i => i.Name,
        TsConstObject { Exported: true } co => co.Name,
        TsNamespaceDeclaration { Exported: true } ns => ns.Name,
        _ => null
    };

    /// <summary>
    /// Returns true if the export is type-only (no runtime value).
    /// Type aliases and interfaces are type-only. Classes, functions, enums, const objects,
    /// and namespaces are values.
    /// </summary>
    private static bool IsTypeOnlyExport(TsTopLevel node) => node is TsTypeAlias or TsInterface;
}
