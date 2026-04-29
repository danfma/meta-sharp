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
}
