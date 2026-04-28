using Metano.Compiler.IR;
using Metano.TypeScript.AST;

namespace Metano.TypeScript.Bridge;

internal static class IrToTsTypeParameterMapper
{
    public static IReadOnlyList<TsTypeParameter>? Convert(
        IReadOnlyList<IrTypeParameter>? typeParameters
    )
    {
        if (typeParameters is null || typeParameters.Count == 0)
            return null;

        return typeParameters
            .Select(tp =>
            {
                TsType? constraint = tp.Constraints is { Count: > 0 } c
                    ? IrToTsTypeMapper.Map(c[0])
                    : null;
                return new TsTypeParameter(tp.Name, constraint);
            })
            .ToList();
    }
}
