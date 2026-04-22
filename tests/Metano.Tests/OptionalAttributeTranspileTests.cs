using Metano.Compiler.Diagnostics;

namespace Metano.Tests;

/// <summary>
/// Tests for <c>[Optional]</c> from <c>Metano.Annotations.TypeScript</c>.
/// The attribute opts a nullable C# parameter or property into the
/// optional-presence TS emission form (<c>name?: T</c>) so a consumer
/// can omit the key entirely. Paired with ADR-0014's loose-equality
/// null-check emission, JS-side <c>undefined</c> collapses to C#
/// <c>null</c> without breaking C#-authored null guards.
/// </summary>
public class OptionalAttributeTranspileTests
{
    // ─── Emission ────────────────────────────────────────────

    [Test]
    public async Task Optional_OnNullablePlainObjectField_EmitsOptionalForm()
    {
        // [PlainObject] records expose their positional parameters as
        // interface fields. [Optional] on a nullable parameter lowers
        // to the TS optional-presence form (`name?: string | null`) so
        // a consumer can construct the shape without the key at all.
        var result = TranspileHelper.Transpile(
            """
            #nullable enable
            using Metano.Annotations.TypeScript;

            [Transpile, PlainObject]
            public record UserDto([Optional] string? Name, int Age);
            """
        );

        var output = result["user-dto.ts"];
        await Assert.That(output).Contains("readonly name?: string | null");
        await Assert.That(output).Contains("readonly age: number");
    }

    [Test]
    public async Task OptionalMissing_OnNullablePlainObjectField_StaysPresentNullable()
    {
        // Without [Optional] a nullable plain-object parameter keeps
        // the present-with-null default — the consumer still has to
        // provide the key, but may set it to null. Baseline for the
        // matrix.
        var result = TranspileHelper.Transpile(
            """
            #nullable enable
            [Transpile, PlainObject]
            public record UserDto(string? Name, int Age);
            """
        );

        var output = result["user-dto.ts"];
        await Assert.That(output).DoesNotContain("name?: string");
        await Assert.That(output).Contains("readonly name: string | null");
    }

    [Test]
    public async Task Optional_OnInterfaceProperty_EmitsOptionalForm()
    {
        // Regular (non-PlainObject) interfaces also honor [Optional] —
        // the property lowers with the `?` suffix so a consumer object
        // implementing the interface can omit the key at construction
        // time.
        var result = TranspileHelper.Transpile(
            """
            #nullable enable
            using Metano.Annotations.TypeScript;

            [Transpile]
            public interface IUserProps
            {
                [Optional] string? Nickname { get; }
                string Name { get; }
            }
            """
        );

        var output = result["i-user-props.ts"];
        await Assert.That(output).Contains("nickname?: string | null");
        await Assert.That(output).Contains("name: string");
    }

    // ─── Diagnostic ──────────────────────────────────────────

    [Test]
    public async Task Optional_OnNonNullableParameter_EmitsMs0010()
    {
        // [Optional] on a non-nullable parameter is rejected — the
        // attribute relies on `undefined` collapsing to `null`, which a
        // non-nullable type cannot represent. The diagnostic points at
        // the offending symbol so the fix (add `?`) is obvious.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            #nullable enable
            using Metano.Annotations.TypeScript;

            [Transpile, PlainObject]
            public record UserDto([Optional] string Name);
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.OptionalRequiresNullable))
            .IsTrue();
    }

    [Test]
    public async Task Optional_OnNullableParameter_EmitsNoDiagnostic()
    {
        // The matching positive case — a nullable parameter with
        // [Optional] is valid and extraction completes without raising
        // MS0010.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            #nullable enable
            using Metano.Annotations.TypeScript;

            [Transpile, PlainObject]
            public record UserDto([Optional] string? Name);
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.OptionalRequiresNullable))
            .IsFalse();
    }
}
