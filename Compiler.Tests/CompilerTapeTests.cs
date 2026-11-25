using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

public class CompilerTapeTests
{
    [Fact]
    public void CompilerSupportsAccessBeyondTheOriginalTapeLimit()
    {
        var instructions = new List<ASTNode>();
        for (var index = 0; index < 3000; index++)
        {
            instructions.Add(new ASTNode(new Token(Token_Enum.MoveRight)));
        }

        for (var index = 0; index < 65; index++)
        {
            instructions.Add(new ASTNode(new Token(Token_Enum.Increment)));
        }

        instructions.Add(new ASTNode(new Token(Token_Enum.Output)));
        var ast = BuildBalancedSequence(instructions, 0, instructions.Count);
        var originalOutput = Console.Out;
        using var output = new StringWriter();

        try
        {
            Console.SetOut(output);
            new CodeGenerator(ast).gen();
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        Assert.Equal("A", output.ToString());
    }

    private static ASTNode BuildBalancedSequence(IReadOnlyList<ASTNode> instructions, int start, int count)
    {
        if (count == 1) return instructions[start];

        var leftCount = count / 2;
        var sequence = new ASTNode
        {
            LeftNode = BuildBalancedSequence(instructions, start, leftCount),
            RightNode = BuildBalancedSequence(instructions, start + leftCount, count - leftCount)
        };

        return sequence;
    }
}
