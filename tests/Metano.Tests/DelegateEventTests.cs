namespace Metano.Tests;

public class DelegateEventTests
{
    [Test]
    public async Task ActionType_MapsToArrowFunctionType()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public class Notifier
            {
                public Action<string>? Callback { get; set; }
            }
            """
        );

        var output = result["notifier.ts"];
        // Action<string> → (obj: string) => void (Roslyn names the param "obj")
        await Assert.That(output).Contains("(obj: string) => void");
    }

    [Test]
    public async Task EventField_EmitsFieldAndAddRemoveMethods()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public class Counter
            {
                public event Action<int>? CountChanged;
            }
            """
        );

        var output = result["counter.ts"];
        // Field: nullable delegate
        await Assert.That(output).Contains("countChanged:");
        await Assert.That(output).Contains("| null = null");
        // $add method
        await Assert.That(output).Contains("countChanged$add(handler:");
        await Assert.That(output).Contains("delegateAdd(");
        // $remove method
        await Assert.That(output).Contains("countChanged$remove(handler:");
        await Assert.That(output).Contains("delegateRemove(");
    }

    [Test]
    public async Task EventField_ImportsRuntimeHelpers()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public class Counter
            {
                public event Action<int>? CountChanged;
            }
            """
        );

        var output = result["counter.ts"];
        await Assert.That(output).Contains("import { delegateAdd, delegateRemove }");
        await Assert.That(output).Contains("from \"metano-runtime\"");
    }

    [Test]
    public async Task EventSubscription_LowersToAddMethod()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public class Counter
            {
                public event Action<int>? CountChanged;
            }

            [Transpile]
            public class App
            {
                private Counter _counter = new Counter();

                public void Setup()
                {
                    _counter.CountChanged += OnCountChanged;
                }

                private void OnCountChanged(int count) { }
            }
            """
        );

        var output = result["app.ts"];
        await Assert.That(output).Contains("countChanged$add(");
    }

    [Test]
    public async Task EventUnsubscription_LowersToRemoveMethod()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public class Counter
            {
                public event Action<int>? CountChanged;
            }

            [Transpile]
            public class App
            {
                private Counter _counter = new Counter();

                public void Teardown()
                {
                    _counter.CountChanged -= OnCountChanged;
                }

                private void OnCountChanged(int count) { }
            }
            """
        );

        var output = result["app.ts"];
        await Assert.That(output).Contains("countChanged$remove(");
    }
}
