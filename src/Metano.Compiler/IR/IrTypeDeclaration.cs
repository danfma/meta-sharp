namespace Metano.Compiler.IR;

/// <summary>
/// A type declaration in the IR. Represents the semantic shape of a C# type
/// without prescribing target-specific rendering.
/// Names stay in their original C# casing.
/// </summary>
public abstract record IrTypeDeclaration(
    string Name,
    IrVisibility Visibility,
    IReadOnlyList<IrTypeParameter>? TypeParameters = null,
    IReadOnlyList<IrAttribute>? Attributes = null
);

/// <summary>
/// A class, struct, or record declaration.
/// </summary>
public sealed record IrClassDeclaration(
    string Name,
    IrVisibility Visibility,
    IrTypeSemantics Semantics,
    IrTypeRef? BaseType = null,
    IReadOnlyList<IrTypeRef>? Interfaces = null,
    IReadOnlyList<IrMemberDeclaration>? Members = null,
    IrConstructorDeclaration? Constructor = null,
    IReadOnlyList<IrTypeDeclaration>? NestedTypes = null,
    IReadOnlyList<IrTypeParameter>? TypeParameters = null,
    IReadOnlyList<IrAttribute>? Attributes = null
) : IrTypeDeclaration(Name, Visibility, TypeParameters, Attributes);

/// <summary>
/// An interface declaration.
/// </summary>
public sealed record IrInterfaceDeclaration(
    string Name,
    IrVisibility Visibility,
    IReadOnlyList<IrTypeRef>? BaseInterfaces = null,
    IReadOnlyList<IrMemberDeclaration>? Members = null,
    IReadOnlyList<IrTypeParameter>? TypeParameters = null,
    IReadOnlyList<IrAttribute>? Attributes = null
) : IrTypeDeclaration(Name, Visibility, TypeParameters, Attributes);

/// <summary>
/// An enum declaration.
/// </summary>
public sealed record IrEnumDeclaration(
    string Name,
    IrVisibility Visibility,
    IReadOnlyList<IrEnumMember> Members,
    IrEnumStyle Style = IrEnumStyle.Numeric,
    IReadOnlyList<IrAttribute>? Attributes = null
) : IrTypeDeclaration(Name, Visibility, Attributes: Attributes);

/// <summary>
/// A delegate declaration — its signature lowers to a TS type alias
/// (<c>export type Name = (…) =&gt; R;</c>) on backends that support
/// named function aliases.
/// </summary>
public sealed record IrDelegateDeclaration(
    string Name,
    IrVisibility Visibility,
    IReadOnlyList<IrParameter> Parameters,
    IrTypeRef ReturnType,
    IrTypeRef? ThisType = null,
    IReadOnlyList<IrTypeParameter>? TypeParameters = null,
    IReadOnlyList<IrAttribute>? Attributes = null
) : IrTypeDeclaration(Name, Visibility, TypeParameters, Attributes);
