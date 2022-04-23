using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using Xunit;
using Xunit.Abstractions;


[assembly: SupportedOSPlatform("windows6.2")]

namespace Switcheroo.Windows.Tests;

public class WindowFinderTests {
    public ITestOutputHelper Output { get; }

    public WindowFinderTests(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public void Test1()
    {
        var sw = new Stopwatch();
        sw.Start();

        var windows = new WindowFinder2().GetWindows().ToList();

        Output.WriteLine($"Time: {sw.Elapsed:g}");

        foreach (var window in windows) {
            Output.WriteLine($"{window.Title}");
        }
    }
}
