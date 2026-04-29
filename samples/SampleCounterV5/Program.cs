// The C# Program type exists so dotnet has an entry point — the
// transpiled output for this file is replaced by `Bindings.ts/program.ts`
// after Metano runs. The C# body calls into the JS-only entry shape
// (`createElement(CounterApp, ...)`) which today's IR pipeline can't
// emit cleanly: `typeof(CounterApp)` doesn't lower, and `[Emit]`
// templates don't drive their own imports. The TS adapter overrides
// the stub with a hand-written entry.
namespace SampleCounterV5;

public static class Program
{
    public static void Main()
    {
        // Real entry runs through the TS-side adapter. .NET-side body
        // stays empty so dotnet build succeeds.
    }
}
