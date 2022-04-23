using System;
using System.Collections.Generic;
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
        IReadOnlyCollection<HWnd> windows = Array.Empty<HWnd>();
        var windowFinder = new WindowFinder();

        var sw = new Stopwatch();
        for (int i = 0; i < 40; i++) {
            sw.Restart();

            windows = windowFinder.GetWindows();

            Output.WriteLine($"Time: {sw.Elapsed.TotalMilliseconds}");
        }

        foreach (var window in windows) {
            Output.WriteLine($"{window.Title}");
        }
    }
}
