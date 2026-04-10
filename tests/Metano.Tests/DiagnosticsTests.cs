using Metano.Compiler.Diagnostics;

namespace Metano.Tests;

public class DiagnosticsTests
{
    [Test]
    public async Task UnsupportedStatement_GeneratesWarning()
    {
        // `lock` statement is not supported by the transpiler
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            [Transpile]
            public class Counter
            {
                private readonly object _gate = new();
                private int _value;

                public void Increment()
                {
                    lock (_gate)
                    {
                        _value++;
                    }
                }
            }
            """
        );

        await Assert.That(diagnostics.Count).IsGreaterThan(0);
        await Assert.That(diagnostics.Any(d =>
            d.Severity == MetanoDiagnosticSeverity.Warning &&
            d.Code == DiagnosticCodes.UnsupportedFeature)).IsTrue();
    }

    [Test]
    public async Task SupportedCode_NoDiagnostics()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            [Transpile]
            public record Money(int Cents, string Currency);
            """
        );

        await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Diagnostic_HasSourceLocation()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            [Transpile]
            public class Foo
            {
                public void Bar()
                {
                    lock (this) { }
                }
            }
            """
        );

        var diag = diagnostics.FirstOrDefault(d => d.Code == DiagnosticCodes.UnsupportedFeature);
        await Assert.That(diag).IsNotNull();
        await Assert.That(diag!.Location).IsNotNull();
    }

    [Test]
    public async Task Format_RoslynStyle()
    {
        var diag = new MetanoDiagnostic(
            MetanoDiagnosticSeverity.Warning,
            "MS0001",
            "Test message",
            null);

        await Assert.That(diag.Format()).IsEqualTo("warning MS0001: Test message");
    }
}
