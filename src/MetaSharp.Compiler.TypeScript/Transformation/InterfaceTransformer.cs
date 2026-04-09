using MetaSharp.Compiler;
using MetaSharp.TypeScript.AST;
using Microsoft.CodeAnalysis;

namespace MetaSharp.Transformation;

/// <summary>
/// Transforms a C# interface into a TypeScript <see cref="TsInterface"/> declaration.
/// Walks public properties and ordinary methods, runs <see cref="SymbolHelper"/> for
/// <c>[Name]</c> overrides + <c>[Ignore]</c> filtering, maps types via
/// <see cref="TypeMapper"/>, and emits a single TS interface.
///
/// Pure / stateless: takes only the symbol + the output statement list.
/// </summary>
public static class InterfaceTransformer
{
    public static void Transform(INamedTypeSymbol type, List<TsTopLevel> statements)
    {
        var properties = new List<TsProperty>();
        var interfaceMethods = new List<TsInterfaceMethod>();

        foreach (var member in type.GetMembers())
        {
            if (member.IsImplicitlyDeclared) continue;
            if (member.DeclaredAccessibility != Accessibility.Public) continue;
            if (SymbolHelper.HasIgnore(member)) continue;

            switch (member)
            {
                case IPropertySymbol prop:
                    var propName = SymbolHelper.GetNameOverride(prop) ?? TypeScriptNaming.ToCamelCase(prop.Name);
                    var propType = TypeMapper.Map(prop.Type);
                    var isReadonly = prop.SetMethod is null || prop.SetMethod.IsInitOnly;
                    properties.Add(new TsProperty(propName, propType, isReadonly));
                    break;

                case IMethodSymbol method when method.MethodKind == MethodKind.Ordinary:
                    var name = SymbolHelper.GetNameOverride(method) ?? TypeScriptNaming.ToCamelCase(method.Name);
                    var returnType = TypeMapper.Map(method.ReturnType);
                    var parameters = method.Parameters
                        .Select(p => new TsParameter(TypeScriptNaming.ToCamelCase(p.Name), TypeMapper.Map(p.Type)))
                        .ToList();
                    var methodTypeParams = TypeTransformer.ExtractMethodTypeParameters(method);
                    interfaceMethods.Add(new TsInterfaceMethod(name, parameters, returnType, methodTypeParams));
                    break;
            }
        }

        var tsName = TypeTransformer.GetTsTypeName(type);
        var typeParams = TypeTransformer.ExtractTypeParameters(type);
        statements.Add(new TsInterface(tsName, properties, TypeParameters: typeParams,
            Methods: interfaceMethods.Count > 0 ? interfaceMethods : null));
    }
}
