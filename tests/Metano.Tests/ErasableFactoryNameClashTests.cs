using Metano.Compiler.Diagnostics;

namespace Metano.Tests;

/// <summary>
/// Tests for <c>MS0020 ErasableFactoryNameClash</c>. The diagnostic
/// fires when an <c>[Erasable]</c> static method's emitted TS name —
/// after <c>[Name]</c> resolution, otherwise camelCase — collides with
/// the TS name of a transpilable type the same emit scope can see, or
/// with another <c>[Erasable]</c> factory across classes. Without the
/// diagnostic the import collector silently resolves the bare
/// identifier to the factory instead of the class, the factory body's
/// <c>new ClassName(...)</c> becomes a recursive call, and consumer
/// files import the function where they meant the type.
/// </summary>
public class ErasableFactoryNameClashTests
{
    [Test]
    public async Task NameOverride_MatchingTranspilableType_RaisesMs0020()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            namespace App;

            [Transpile]
            public sealed class Column
            {
                public Column(int gap) { }
            }

            [Transpile, Erasable]
            public static class UI
            {
                [ObjectArgs, Name("Column")]
                public static Column Column(int gap) => new(gap);
            }
            """
        );

        var ms0020 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.ErasableFactoryNameClash
        );
        await Assert.That(ms0020).IsNotNull();
        await Assert.That(ms0020!.Message).Contains("Column");
        await Assert.That(ms0020.Message).Contains("UI.Column");
    }

    [Test]
    public async Task NameOverride_NotMatchingAnyType_NoDiagnostic()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            namespace App;

            [Transpile]
            public sealed class Column
            {
                public Column(int gap) { }
            }

            [Transpile, Erasable]
            public static class UI
            {
                [ObjectArgs, Name("makeColumn")]
                public static Column Column(int gap) => new(gap);
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.ErasableFactoryNameClash))
            .IsFalse();
    }

    [Test]
    public async Task DefaultCamelCase_DoesNotClashWithPascalCaseType_NoDiagnostic()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            namespace App;

            [Transpile]
            public sealed class Column
            {
                public Column(int gap) { }
            }

            [Transpile, Erasable]
            public static class UI
            {
                [ObjectArgs]
                public static Column Column(int gap) => new(gap);
            }
            """
        );

        await Assert
            .That(diagnostics.Any(d => d.Code == DiagnosticCodes.ErasableFactoryNameClash))
            .IsFalse();
    }

    // Cross-assembly clash detection follows the cross-assembly Erasable
    // discovery work in issue #178; today the detector scans only the
    // current assembly's transpilable type table.

    [Test]
    public async Task NameOverride_MatchingRuntimeHelper_RaisesMs0020()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            namespace App;

            [Transpile, Erasable]
            public static class Codecs
            {
                [Name("Enumerable")]
                public static int Wrap(int x) => x;
            }
            """
        );

        var ms0020 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.ErasableFactoryNameClash
        );
        await Assert.That(ms0020).IsNotNull();
        await Assert.That(ms0020!.Message).Contains("metano-runtime");
    }

    [Test]
    public async Task TwoErasableFactoriesSameName_RaisesMs0020()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            namespace App;

            [Transpile, Erasable]
            public static class Containers
            {
                [Name("box")]
                public static int OneBox(int x) => x;
            }

            [Transpile, Erasable]
            public static class Wrappers
            {
                [Name("box")]
                public static int AnotherBox(int x) => x;
            }
            """
        );

        var ms0020 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.ErasableFactoryNameClash
        );
        await Assert.That(ms0020).IsNotNull();
        await Assert.That(ms0020!.Message).Contains("box");
    }
}
