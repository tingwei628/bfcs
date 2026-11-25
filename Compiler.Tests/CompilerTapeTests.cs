using System;
using System.IO;
using Xunit;

[Collection("Console output")]
public class CompilerTapeTests
{
    [Fact]
    public void CompilerPreservesExistingCellsWhenTapeGrows()
    {
        var source = new string('>', 2999) + new string('+', 'A') + "." +
            ">" + new string('+', 'B') + ".<.";

        Assert.Equal("ABA", CompileAndCaptureOutput(source));
    }

    [Fact]
    public void CompilerGrowsTapeWhenALoopRepeatsRightMoves()
    {
        var right = new string('>', 50);
        var left = new string('<', 50);
        // Transfer the counter 50 cells to the right, then decrement it.
        // After 125 iterations the pointer is at index 6250.
        var source = new string('+', 125) + "[[-" + right + "+" + left +
            "]" + right + "-]" + new string('+', 'A') + ".";

        Assert.Equal("A", CompileAndCaptureOutput(source));
    }

    private static string CompileAndCaptureOutput(string source)
    {
        var path = Path.GetTempFileName();
        var originalOutput = Console.Out;
        using var output = new StringWriter();

        try
        {
            File.WriteAllText(path, source);
            Console.SetOut(output);
            new BrainFuckCompiler(new[] { path }).compile();
            return output.ToString();
        }
        finally
        {
            Console.SetOut(originalOutput);
            File.Delete(path);
        }
    }
}
