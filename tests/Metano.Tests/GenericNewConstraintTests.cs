namespace Metano.Tests;

public class GenericNewConstraintTests
{
    [Test]
    public async Task NewOnTypeParameter_EmitsMs0019()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            namespace App;

            [Transpile]
            public static class Factory
            {
                public static T Make<T>() where T : new() => new T();
            }
            """
        );

        var ms0019 = diagnostics.FirstOrDefault(d =>
            d.Code == Metano.Compiler.Diagnostics.DiagnosticCodes.GenericNewConstraint
        );
        await Assert.That(ms0019).IsNotNull();
        await Assert.That(ms0019!.Message).Contains("new T()");
        await Assert
            .That(ms0019.Severity)
            .IsEqualTo(Metano.Compiler.Diagnostics.MetanoDiagnosticSeverity.Error);
    }

    [Test]
    public async Task NewOnConcreteType_DoesNotEmitMs0019()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            namespace App;

            [Transpile]
            public class Box { }

            [Transpile]
            public static class Factory
            {
                public static Box Make() => new Box();
            }
            """
        );

        var ms0019 = diagnostics.FirstOrDefault(d =>
            d.Code == Metano.Compiler.Diagnostics.DiagnosticCodes.GenericNewConstraint
        );
        await Assert.That(ms0019).IsNull();
    }
}
