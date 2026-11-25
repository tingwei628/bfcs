using System;
using System.Diagnostics;
using System.IO;

internal static class Frontend
{
    private static void Main(string[] args)
    {
        var source = File.ReadAllText(args[0]);

        // Warm up the methods without processing the measured source.
        Parse("+[>+<-]");
        var timer = new Stopwatch();
        long before = GC.GetAllocatedBytesForCurrentThread();
        timer.Start();
        var tree = Parse(source);
        timer.Stop();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(tree);

        Console.WriteLine(FormattableString.Invariant($"{timer.Elapsed.TotalMilliseconds:F4} {allocated}"));
    }

    private static ASTNode Parse(string source)
    {
        return new Parser(new Lexer(source).lex()).ast();
    }
}
