namespace Metano.Tests;

public class ExtensionCallSiteTests
{
    [Test]
    public async Task ClassicExtension_CalledViaReceiver_LowersToHelperCall()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public static class IntExt
            {
                public static int Squared(this int x) => x * x;
            }

            [Transpile]
            public class Calc
            {
                public int Run(int n) => n.Squared();
            }
            """
        );

        var output = result["calc.ts"];
        await Assert.That(output).Contains("squared(n)");
        await Assert.That(output).DoesNotContain("n.squared");
    }

    [Test]
    public async Task ExtensionBlock_MethodCalledViaReceiver_LowersToHelperCall()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public static class IntExtensions
            {
                extension(int value)
                {
                    public int Doubled() => value * 2;
                }
            }

            [Transpile]
            public class Calc
            {
                public int Run(int n) => n.Doubled();
            }
            """
        );

        var output = result["calc.ts"];
        await Assert.That(output).Contains("doubled(n)");
        await Assert.That(output).DoesNotContain("n.doubled");
    }

    [Test]
    public async Task ExtensionBlock_PropertyRead_LowersToGetterCall()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public static class IntExtensions
            {
                extension(int value)
                {
                    public bool IsEven => value % 2 == 0;
                }
            }

            [Transpile]
            public class Calc
            {
                public bool Run(int n) => n.IsEven;
            }
            """
        );

        var output = result["calc.ts"];
        await Assert.That(output).Contains("isEven$get(n)");
        await Assert.That(output).DoesNotContain(".isEven");
    }

    [Test]
    public async Task ClassicExtension_WithNameOverride_CallSiteHonorsRename()
    {
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations;
            namespace App;

            [Transpile]
            public static class IntExt
            {
                [Name("twice")]
                public static int Double(this int x) => x + x;
            }

            [Transpile]
            public class Calc
            {
                public int Run(int n) => n.Double();
            }
            """
        );

        var caller = result["calc.ts"];
        await Assert.That(caller).Contains("twice(n)");
        await Assert.That(caller).DoesNotContain("double(n)");
        await Assert.That(caller).Contains("import { twice } from \"./int-ext\";");
    }

    [Test]
    public async Task ExtensionHelper_LandsInSiblingFile_ImportsAtCallSite()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public static class IntExt
            {
                public static int Squared(this int x) => x * x;
            }

            [Transpile]
            public class Calc
            {
                public int Run(int n) => n.Squared();
            }
            """
        );

        var caller = result["calc.ts"];
        await Assert.That(caller).Contains("squared(n)");
        await Assert.That(caller).Contains("import { squared } from \"./int-ext\";");
    }

    [Test]
    public async Task TwoExtensionsSameEmittedName_RaisesMs0021()
    {
        var (_, diagnostics) = TranspileHelper.TranspileWithDiagnostics(
            """
            using Metano.Annotations;
            namespace App;

            [Transpile]
            public static class IntExt
            {
                public static int Squared(this int x) => x * x;
            }

            [Transpile]
            public static class LongExt
            {
                public static long Squared(this long x) => x * x;
            }

            [Transpile]
            public class Calc
            {
                public int Run(int n) => n.Squared();
            }
            """
        );

        var ms0021 = diagnostics.FirstOrDefault(d =>
            d.Code == Metano.Compiler.Diagnostics.DiagnosticCodes.ExtensionHelperNameClash
        );
        await Assert.That(ms0021).IsNotNull();
        await Assert.That(ms0021!.Message).Contains("squared");
    }
}
