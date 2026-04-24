using Metano.Compiler.Diagnostics;

namespace Metano.Tests;

/// <summary>
/// Tests for the opt-in <c>--strip-interface-prefix</c> flag (issue
/// #92). When enabled, the transformer rewrites every interface
/// whose C# name matches <c>^I[A-Z]</c> to drop the leading <c>I</c>
/// so generated TypeScript matches community conventions
/// (<c>IIssueRepository</c> → <c>IssueRepository</c>). Collisions
/// with sibling types in the same namespace keep the prefix and
/// raise <c>MS0017</c>. Explicit
/// <c>[Name(TypeScript, "…")]</c> overrides win over the strip.
/// </summary>
public class StripInterfacePrefixTests
{
    [Test]
    public async Task StripInterfacePrefix_NoCollision_DropsPrefix()
    {
        var (files, _) = TranspileHelper.TranspileWithStripInterfacePrefix(
            """
            [assembly: TranspileAssembly]

            public interface IIssueRepository
            {
                string Name { get; }
            }
            """
        );

        await Assert.That(files).ContainsKey("issue-repository.ts");
        await Assert.That(files).DoesNotContainKey("i-issue-repository.ts");
        var output = files["issue-repository.ts"];
        await Assert.That(output).Contains("export interface IssueRepository");
        await Assert.That(output).DoesNotContain("IIssueRepository");
    }

    [Test]
    public async Task StripInterfacePrefix_OffByDefault_PreservesPrefix()
    {
        // Regression guard: without the flag the historical shape is
        // preserved byte-for-byte so consumer imports do not break
        // on upgrade.
        var files = TranspileHelper.Transpile(
            """
            [assembly: TranspileAssembly]

            public interface IIssueRepository
            {
                string Name { get; }
            }
            """
        );

        await Assert.That(files).ContainsKey("i-issue-repository.ts");
        await Assert.That(files["i-issue-repository.ts"]).Contains("IIssueRepository");
    }

    [Test]
    public async Task StripInterfacePrefix_CollidesWithSiblingClass_KeepsPrefixAndEmitsMs0017()
    {
        var (files, diagnostics) = TranspileHelper.TranspileWithStripInterfacePrefix(
            """
            [assembly: TranspileAssembly]

            public interface IIssueRepository { string Name { get; } }

            public class IssueRepository
            {
                public string Name => "fixed";
            }
            """
        );

        // Conflict → prefix preserved so neither consumer import
        // collapses into the other.
        await Assert.That(files).ContainsKey("i-issue-repository.ts");
        await Assert.That(files).ContainsKey("issue-repository.ts");

        var ms0017 = diagnostics.FirstOrDefault(d =>
            d.Code == DiagnosticCodes.InterfacePrefixCollision
        );
        await Assert.That(ms0017).IsNotNull();
        await Assert.That(ms0017!.Message).Contains("IIssueRepository");
        await Assert.That(ms0017!.Message).Contains("IssueRepository");
    }

    [Test]
    public async Task StripInterfacePrefix_NameOverride_WinsOverStrip()
    {
        // `[Name(TypeScript, "…")]` already requested a specific
        // emitted name; the strip pass must leave it alone.
        var (files, _) = TranspileHelper.TranspileWithStripInterfacePrefix(
            """
            using Metano.Annotations;
            [assembly: TranspileAssembly]

            [Name(TargetLanguage.TypeScript, "IRepo")]
            public interface IIssueRepository { string Name { get; } }
            """
        );

        await Assert.That(files).ContainsKey("i-repo.ts");
        await Assert.That(files["i-repo.ts"]).Contains("export interface IRepo");
    }

    [Test]
    public async Task StripInterfacePrefix_IdentityShape_DoesNotMatch()
    {
        // `Identity` lacks the `I[A-Z]` shape (second char is lowercase);
        // the regex rejects it so the strip does not over-reach.
        var (files, _) = TranspileHelper.TranspileWithStripInterfacePrefix(
            """
            [assembly: TranspileAssembly]

            public interface Identity { string Name { get; } }
            """
        );

        await Assert.That(files).ContainsKey("identity.ts");
        await Assert.That(files["identity.ts"]).Contains("export interface Identity");
    }

    [Test]
    public async Task StripInterfacePrefix_ConsumerImportsUseStrippedName()
    {
        // Integration: a separate transpilable class references the
        // interface. The consumer must import the stripped name from
        // the stripped file path; without mirroring the rewrite into
        // TranspilableTypes, the import collector would emit the
        // original `IIssueRepository` / `i-issue-repository` pair and
        // the generated TS would break.
        var (files, _) = TranspileHelper.TranspileWithStripInterfacePrefix(
            """
            [assembly: TranspileAssembly]

            public interface IIssueRepository
            {
                string Name { get; }
            }

            public class Service
            {
                private readonly IIssueRepository _repo;
                public Service(IIssueRepository repo) { _repo = repo; }
                public string Name => _repo.Name;
            }
            """
        );

        await Assert.That(files).ContainsKey("service.ts");
        var output = files["service.ts"];
        await Assert.That(output).Contains("IssueRepository");
        await Assert.That(output).Contains("./issue-repository");
        await Assert.That(output).DoesNotContain("IIssueRepository");
        await Assert.That(output).DoesNotContain("i-issue-repository");
    }

    [Test]
    public async Task StripInterfacePrefix_DoesNotTouchClasses()
    {
        // `IpAddress` is a class, not an interface — its name must
        // not be rewritten even though it would match the regex.
        var (files, _) = TranspileHelper.TranspileWithStripInterfacePrefix(
            """
            [assembly: TranspileAssembly]

            public class IPAddress
            {
                public string Value => "127.0.0.1";
            }
            """
        );

        await Assert.That(files).ContainsKey("ip-address.ts");
        await Assert.That(files["ip-address.ts"]).Contains("export class IPAddress");
    }
}
