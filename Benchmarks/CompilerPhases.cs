using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

internal static class CompilerPhases
{
    private static void Main(string[] args)
    {
        var source = File.ReadAllText(args[0]);

        // Warm up the harness and generator using a separate generated method.
        Measure("+[>+<-]");
        Console.WriteLine(FormattableString.Invariant(Measure(source)));
    }

    private static FormattableString Measure(string source)
    {
        var timer = new Stopwatch();
        long before = GC.GetAllocatedBytesForCurrentThread();
        timer.Start();
        var tree = new Parser(new Lexer(source).lex()).ast();
        timer.Stop();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        double frontendMs = timer.Elapsed.TotalMilliseconds;

        timer.Restart();
        var method = new CodeGenerator(tree).GenerateMethod();
        timer.Stop();
        double emitMs = timer.Elapsed.TotalMilliseconds;

        timer.Restart();
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        timer.Stop();
        double jitMs = timer.Elapsed.TotalMilliseconds;

        timer.Restart();
        method.Invoke(null, null);
        timer.Stop();
        double executeMs = timer.Elapsed.TotalMilliseconds;

        return $"{frontendMs:F4} {allocated} {emitMs:F4} {jitMs:F4} {executeMs:F4}";
    }
}
