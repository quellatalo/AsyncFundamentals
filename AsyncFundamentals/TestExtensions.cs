using Newtonsoft.Json;

namespace AsyncFundamentals;

public static class TestExtensions
{
    /// <summary>
    /// Prints data to console.
    /// </summary>
    /// <param name="instance">The object to print.</param>
    public static void Dump(this object? instance)
        => Console.Out.WriteLine(instance as string ?? JsonConvert.SerializeObject(instance, Formatting.Indented));

    /// <summary>
    /// Prints data to console, with Thread ID as prefix.
    /// </summary>
    /// <param name="instance">The object to print.</param>
    public static void ThreadDump(this object? instance)
        => Console.Out.WriteLine(
            $"{Environment.CurrentManagedThreadId}: {instance as string ?? JsonConvert.SerializeObject(instance, Formatting.Indented)}");
}
