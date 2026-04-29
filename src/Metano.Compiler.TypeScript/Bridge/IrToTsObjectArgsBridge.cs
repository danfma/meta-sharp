using Metano.Compiler.IR;
using Metano.Transformation;
using Metano.TypeScript.AST;

namespace Metano.TypeScript.Bridge;

/// <summary>
/// Lowers <c>[ObjectArgs]</c>-annotated parameter lists into the synthetic
/// <c>args: { p1: T1; p2?: T2 }</c> shape plus the destructuring header
/// statement. Shared by module-function and class-method emission.
/// </summary>
/// <remarks>
/// Every C# parameter becomes a field on the synthesized object type;
/// optional ones (<see cref="IrParameter.IsOptional"/> or carrying a
/// default value) render with the <c>?:</c> suffix so callers may omit
/// them. The destructure header restores the original parameter names so
/// the lowered body keeps working unchanged.
/// </remarks>
internal static class IrToTsObjectArgsBridge
{
    public static bool HasObjectArgs(IReadOnlyList<IrAttribute>? attributes) =>
        attributes is { Count: > 0 } list
        && list.Any(a => a.Name is "ObjectArgsAttribute" or "ObjectArgs");

    public static (TsParameter ArgsParam, TsRawStatement DestructureHeader) BuildArgsParam(
        IReadOnlyList<IrParameter> parameters,
        DeclarativeMappingRegistry? bclRegistry
    )
    {
        var fieldFragments = new List<string>();
        var destructuringFragments = new List<string>();
        foreach (var p in parameters)
        {
            var pName = TypeScriptNaming.ToCamelCase(p.Name);
            var pType = Printer.RenderType(IrToTsTypeMapper.Map(p.Type)).Trim();
            var isOptional = p.IsOptional || p.HasDefaultValue;
            fieldFragments.Add($"{pName}{(isOptional ? "?" : "")}: {pType}");

            if (p.HasDefaultValue && p.DefaultValue is not null)
            {
                var defaultText = Printer
                    .RenderExpression(IrToTsExpressionBridge.Map(p.DefaultValue, bclRegistry))
                    .Trim();
                destructuringFragments.Add($"{pName} = {defaultText}");
            }
            else
            {
                destructuringFragments.Add(pName);
            }
        }

        var argsType = new TsNamedType("{ " + string.Join("; ", fieldFragments) + " }");
        var argsParam = new TsParameter("args", argsType);
        var destructure = new TsRawStatement(
            "const { " + string.Join(", ", destructuringFragments) + " } = args;"
        );
        return (argsParam, destructure);
    }
}
