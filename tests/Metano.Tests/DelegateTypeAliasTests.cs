using Metano.Compiler.Diagnostics;

namespace Metano.Tests;

public class DelegateTypeAliasTests
{
    [Test]
    public async Task TranspilableVoidDelegate_EmitsTypeAlias()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public delegate void Notifier(string message);
            """
        );

        var output = result["notifier.ts"];
        await Assert.That(output).Contains("export type Notifier = (message: string) => void;");
    }

    [Test]
    public async Task TranspilableReturningDelegate_EmitsTypeAlias()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public delegate int Folder(int acc, int next);
            """
        );

        var output = result["folder.ts"];
        await Assert
            .That(output)
            .Contains("export type Folder = (acc: number, next: number) => number;");
    }

    [Test]
    public async Task GenericDelegate_EmitsAliasWithTypeParameters()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public delegate TOut Converter<TIn, TOut>(TIn value);
            """
        );

        var output = result["converter.ts"];
        await Assert
            .That(output)
            .Contains("export type Converter<TIn, TOut> = (value: TIn) => TOut;");
    }

    [Test]
    public async Task GenericDelegate_WithConstraint_EmitsExtendsClause()
    {
        var result = TranspileHelper.Transpile(
            """
            using System;

            namespace App;

            [Transpile]
            public interface ICloneable<T> {}

            [Transpile]
            public delegate T Cloner<T>(T value) where T : ICloneable<T>;
            """
        );

        var output = result["cloner.ts"];
        await Assert.That(output).Contains("export type Cloner<T extends ICloneable<T>>");
    }

    [Test]
    public async Task ConsumerProperty_UsesAliasNameInsteadOfInlining()
    {
        var result = TranspileHelper.Transpile(
            """
            namespace App;

            [Transpile]
            public delegate void ClickHandler();

            [Transpile]
            public sealed class Button
            {
                public ClickHandler? OnClick { get; set; }
            }
            """
        );

        var button = result["button.ts"];
        await Assert.That(button).Contains("import type { ClickHandler }");
        await Assert.That(button).Contains("onClick: ClickHandler | null = null");
    }

    [Test]
    public async Task AmbientDelegate_StillInlinesAtUsageSite()
    {
        var result = TranspileHelper.Transpile(
            """
            using System;

            namespace App;

            [Transpile]
            public sealed class Button
            {
                public Action<string>? OnClick { get; set; }
            }
            """
        );

        var output = result["button.ts"];
        await Assert.That(output).Contains("(obj: string) => void");
    }

    [Test]
    public async Task AssemblyWideTranspile_PicksUpPublicDelegate()
    {
        var result = TranspileHelper.Transpile(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            namespace App;

            public delegate void Listener(int value);

            public sealed class Source
            {
                public Listener? OnFire { get; set; }
            }
            """
        );

        await Assert.That(result.ContainsKey("listener.ts")).IsTrue();
        var alias = result["listener.ts"];
        await Assert.That(alias).Contains("export type Listener = (value: number) => void;");
        var source = result["source.ts"];
        await Assert.That(source).Contains("import type { Listener }");
    }

    [Test]
    public async Task DartTarget_TranspileDelegate_FallsBackWithDiagnostic()
    {
        var (files, diagnostics) = TranspileHelper.TranspileDart(
            """
            using Metano.Annotations;

            namespace App;

            [Transpile]
            public delegate void Notifier(string message);
            """
        );

        await Assert.That(files).DoesNotContainKey("notifier.dart");
        var fallback = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.UnsupportedFeature
            && d.Message.Contains("Notifier", StringComparison.Ordinal)
        );
        await Assert.That(fallback).IsNotNull();
    }

    [Test]
    public async Task CrossPackageDelegate_EmitsImportFromPackage()
    {
        var library = """
            [assembly: TranspileAssembly]
            [assembly: EmitPackage("@scope/lib")]

            namespace MyLib.Events
            {
                public delegate void Handler(int code);
            }

            namespace MyLib.Other
            {
                public record Marker(string Tag);
            }
            """;

        var consumer = """
            [assembly: TranspileAssembly]

            namespace App;

            public class Subscriber
            {
                public MyLib.Events.Handler? Callback { get; set; }
            }
            """;

        var result = TranspileHelper.TranspileWithLibrary(library, consumer);
        var output = result["subscriber.ts"];
        await Assert.That(output).Contains("import type { Handler } from \"@scope/lib/events\"");
        await Assert.That(output).Contains("callback: Handler | null = null");
    }
}
