using Metano.Annotations;

namespace SampleCounter;

[Erasable]
public static class Program
{
    [ModuleEntryPoint]
    public static void Main()
    {
        Console.WriteLine("Hello, World!");
    }
}
