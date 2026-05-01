using Metano.Compiler.Diagnostics;

namespace Metano.Tests;

/// <summary>
/// Tests for the redefined <c>[Ignore]</c> contract (#106 PR3): the
/// type is painted as .NET-only and any reference from a transpilable
/// type's signature OR body raises <c>MS0013
/// IgnoreReferencedByTranspiledCode</c>. Ambient TS shapes used to
/// share <c>[Ignore]</c> historically; those migrated to
/// <c>[External]</c> in PR2 and stay covered by
/// <see cref="ExternalAmbientTranspileTests"/>.
/// </summary>
public class IgnoreDotNetOnlyTests
{
    [Test]
    public async Task Ignore_ReferencedFromMethodParameter_RaisesMs0013()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [Ignore]
            public class Marker {}

            public class Consumer
            {
                public void Use(Marker m) {}
            }
            """
        );

        var ms0013 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.IgnoreReferencedByTranspiledCode
        );
        await Assert.That(ms0013).IsNotNull();
        await Assert.That(ms0013!.Message).Contains("Marker");
    }

    [Test]
    public async Task Ignore_ReferencedFromMethodReturn_RaisesMs0013()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [Ignore]
            public class Marker {}

            public class Consumer
            {
                public Marker Get() => throw null!;
            }
            """
        );

        var ms0013 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.IgnoreReferencedByTranspiledCode
        );
        await Assert.That(ms0013).IsNotNull();
    }

    [Test]
    public async Task Ignore_ReferencedAsFieldType_RaisesMs0013()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [Ignore]
            public class Marker {}

            public class Consumer
            {
                private Marker? _m;
            }
            """
        );

        var ms0013 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.IgnoreReferencedByTranspiledCode
        );
        await Assert.That(ms0013).IsNotNull();
    }

    [Test]
    public async Task Ignore_ReferencedInsideMethodBody_RaisesMs0013()
    {
        // Body-side: `new Marker()` reaches inside the method body, not
        // the signature. PR3 spec demands transitive painting.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [Ignore]
            public class Marker {}

            public class Consumer
            {
                public int Side()
                {
                    var m = new Marker();
                    return 0;
                }
            }
            """
        );

        var ms0013 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.IgnoreReferencedByTranspiledCode
        );
        await Assert.That(ms0013).IsNotNull();
    }

    [Test]
    public async Task Ignore_GenericTypeArgument_RaisesMs0013()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            using System.Collections.Generic;
            [assembly: TranspileAssembly]

            [Ignore]
            public class Marker {}

            public class Consumer
            {
                public List<Marker> Get() => throw null!;
            }
            """
        );

        var ms0013 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.IgnoreReferencedByTranspiledCode
        );
        await Assert.That(ms0013).IsNotNull();
    }

    [Test]
    public async Task Ignore_NonTranspilableContainer_DoesNotRaise()
    {
        // A `[Ignore]` type referencing another `[Ignore]` type stays
        // silent — both live entirely on the .NET side.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;

            [Ignore]
            public class Marker {}

            [Ignore]
            public class Holder
            {
                public Marker Field => throw null!;
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.IgnoreReferencedByTranspiledCode))
            .IsFalse();
    }

    [Test]
    public async Task Ignore_PerTargetDart_DoesNotRaiseInTypeScript()
    {
        // `[Ignore(TargetLanguage.Dart)]` paints the type as .NET-only
        // on Dart only — TypeScript runs continue to emit it normally.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [Ignore(TargetLanguage.Dart)]
            public class TypeScriptOnly {}

            public class Consumer
            {
                public TypeScriptOnly Use() => throw null!;
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.IgnoreReferencedByTranspiledCode))
            .IsFalse();
    }
}
