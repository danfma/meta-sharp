namespace MetaSharp.Tests;

/// <summary>
/// Tests for the distinction between variable / parameter identifier escape rules
/// and class member declaration / property access escape rules. Reserved words can
/// appear in property position in JS (<c>obj.delete</c>, <c>class Foo { delete() {} }</c>)
/// but NOT as variable identifiers or namespace function names.
///
/// The compiler routes class methods + member-access call sites through a
/// non-escaping helper so the user can write a method named <c>Delete</c> in C# and
/// get <c>delete()</c> in TS without manually adding <c>[Name("delete")]</c>.
/// [InlineWrapper] types stay on the escaping path because they lower to namespace
/// function declarations, where reserved words are still illegal.
/// </summary>
public class MemberNameEscapeTests
{
    [Test]
    public async Task ClassMethodWithReservedName_NotEscaped()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public class Bag
            {
                public void Delete() { }
                public void New() { }
            }
            """);

        var output = result["bag.ts"];
        // Method declarations don't get the underscore.
        await Assert.That(output).Contains("delete():");
        await Assert.That(output).Contains("new():");
        await Assert.That(output).DoesNotContain("delete_");
        await Assert.That(output).DoesNotContain("new_");
    }

    [Test]
    public async Task ClassMethodCallSite_AlsoNotEscaped()
    {
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public class Bag
            {
                public void Delete() { }
            }

            [Transpile]
            public class User
            {
                public void Cleanup(Bag b) => b.Delete();
            }
            """);

        var output = result["user.ts"];
        await Assert.That(output).Contains("b.delete()");
        await Assert.That(output).DoesNotContain("b.delete_()");
    }

    [Test]
    public async Task InlineWrapper_StaticMethodNamed_New_StillEscaped()
    {
        // [InlineWrapper] types lower to namespace functions, where reserved words
        // are illegal even though `obj.new` is fine. Both the declaration and the
        // call site escape so they stay in agreement.
        var result = TranspileHelper.Transpile(
            """
            [Transpile, InlineWrapper]
            public readonly record struct UserId(string Value)
            {
                public static UserId New() => new("x");
            }

            [Transpile]
            public class Service
            {
                public UserId Make() => UserId.New();
            }
            """);

        var idOutput = result["user-id.ts"];
        var svcOutput = result["service.ts"];
        // Declaration uses the escaped form because namespace `function new() {}`
        // is a parse error.
        await Assert.That(idOutput).Contains("function new_");
        // Call site matches.
        await Assert.That(svcOutput).Contains("UserId.new_()");
    }

    [Test]
    public async Task NameOverride_BypassesAllEscaping()
    {
        // [Name("delete")] writes the value verbatim — no escape, no camelCase.
        // This stays the user's escape hatch for properties / fields where the
        // declaration site can't safely use a reserved word.
        var result = TranspileHelper.Transpile(
            """
            [Transpile]
            public class Bag
            {
                [Name("delete")]
                public void Remove() { }
            }

            [Transpile]
            public class User
            {
                public void Cleanup(Bag b) => b.Remove();
            }
            """);

        await Assert.That(result["bag.ts"]).Contains("delete():");
        await Assert.That(result["user.ts"]).Contains("b.delete()");
    }
}
