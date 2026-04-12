using Metano.Annotations;

namespace SampleCounter;

[ExportedAsModule]
public static class Program
{
    [ModuleEntryPoint]
    public static void Main()
    {
        Console.WriteLine("Hello, World!");
    }
}
