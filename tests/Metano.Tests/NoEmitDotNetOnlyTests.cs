using Metano.Compiler.Diagnostics;

namespace Metano.Tests;

/// <summary>
/// Tests for the redefined <c>[NoEmit]</c> contract (#106 PR3): the
/// type is painted as .NET-only and any reference from a transpilable
/// type's signature OR body raises <c>MS0013
/// NoEmitReferencedByTranspiledCode</c>. Ambient TS shapes used to
/// share <c>[NoEmit]</c> historically; those migrated to
/// <c>[External]</c> in PR2 and stay covered by
/// <see cref="NoEmitTranspileTests"/>.
/// </summary>
public class NoEmitDotNetOnlyTests
{
    [Test]
    public async Task NoEmit_ReferencedFromMethodParameter_RaisesMs0013()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [NoEmit]
            public class Marker {}

            public class Consumer
            {
                public void Use(Marker m) {}
            }
            """
        );

        var ms0013 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.NoEmitReferencedByTranspiledCode
        );
        await Assert.That(ms0013).IsNotNull();
        await Assert.That(ms0013!.Message).Contains("Marker");
    }

    [Test]
    public async Task NoEmit_ReferencedFromMethodReturn_RaisesMs0013()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [NoEmit]
            public class Marker {}

            public class Consumer
            {
                public Marker Get() => throw null!;
            }
            """
        );

        var ms0013 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.NoEmitReferencedByTranspiledCode
        );
        await Assert.That(ms0013).IsNotNull();
    }

    [Test]
    public async Task NoEmit_ReferencedAsFieldType_RaisesMs0013()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [NoEmit]
            public class Marker {}

            public class Consumer
            {
                private Marker? _m;
            }
            """
        );

        var ms0013 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.NoEmitReferencedByTranspiledCode
        );
        await Assert.That(ms0013).IsNotNull();
    }

    [Test]
    public async Task NoEmit_ReferencedInsideMethodBody_RaisesMs0013()
    {
        // Body-side: `new Marker()` reaches inside the method body, not
        // the signature. PR3 spec demands transitive painting.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [NoEmit]
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
            d.Code == DiagnosticCodes.NoEmitReferencedByTranspiledCode
        );
        await Assert.That(ms0013).IsNotNull();
    }

    [Test]
    public async Task NoEmit_GenericTypeArgument_RaisesMs0013()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            using System.Collections.Generic;
            [assembly: TranspileAssembly]

            [NoEmit]
            public class Marker {}

            public class Consumer
            {
                public List<Marker> Get() => throw null!;
            }
            """
        );

        var ms0013 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.NoEmitReferencedByTranspiledCode
        );
        await Assert.That(ms0013).IsNotNull();
    }

    [Test]
    public async Task NoEmit_NonTranspilableContainer_DoesNotRaise()
    {
        // A `[NoEmit]` type referencing another `[NoEmit]` type stays
        // silent — both live entirely on the .NET side.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;

            [NoEmit]
            public class Marker {}

            [NoEmit]
            public class Holder
            {
                public Marker Field => throw null!;
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.NoEmitReferencedByTranspiledCode))
            .IsFalse();
    }

    [Test]
    public async Task NoEmit_PerTargetDart_DoesNotRaiseInTypeScript()
    {
        // `[NoEmit(TargetLanguage.Dart)]` paints the type as .NET-only
        // on Dart only — TypeScript runs continue to emit it normally.
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [NoEmit(TargetLanguage.Dart)]
            public class TypeScriptOnly {}

            public class Consumer
            {
                public TypeScriptOnly Use() => throw null!;
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.NoEmitReferencedByTranspiledCode))
            .IsFalse();
    }
}
