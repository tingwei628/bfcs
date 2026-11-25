using System;
using System.IO;
using Xunit;

[Collection("Console output")]
public class ParserTests
{
    [Fact]
    public void LexerIgnoresCharactersOutsideBrainfuckInstructionSet()
    {
        var tokens = new Lexer("+ hello! [->+<] // comment").lex();

        Assert.Collection(tokens,
            token => Assert.Equal(Token_Enum.Increment, token.TokenType),
            token => Assert.Equal(Token_Enum.BeginLoop, token.TokenType),
            token => Assert.Equal(Token_Enum.Decrement, token.TokenType),
            token => Assert.Equal(Token_Enum.MoveRight, token.TokenType),
            token => Assert.Equal(Token_Enum.Increment, token.TokenType),
            token => Assert.Equal(Token_Enum.MoveLeft, token.TokenType),
            token => Assert.Equal(Token_Enum.EndLoop, token.TokenType));
    }

    [Theory]
    [InlineData("[")]
    [InlineData("]")]
    [InlineData("[]]")]
    [InlineData("[[ ]")]
    public void ParserRejectsUnmatchedBrackets(string source)
    {
        var parser = new Parser(new Lexer(source).lex());

        Assert.Throws<Exception>(() => parser.ast());
    }

    [Fact]
    public void ParserCombinesRepeatedCommandsWithoutCrossingLoopBoundaries()
    {
        var ast = new Parser(new Lexer("+++[--]>>>").lex()).ast();

        Assert.Equal(Token_Enum.Increment, ast.LeftNode.TokenType);
        Assert.Equal(3, ast.LeftNode.RepeatCount);
        Assert.Equal(Token_Enum.Decrement, ast.RightNode.LeftNode.MiddleNode.TokenType);
        Assert.Equal(2, ast.RightNode.LeftNode.MiddleNode.RepeatCount);
        Assert.Equal(Token_Enum.MoveRight, ast.RightNode.RightNode.TokenType);
        Assert.Equal(3, ast.RightNode.RightNode.RepeatCount);
    }

    [Fact]
    public void CompilerParsesNestedLoopsAndEmitsExpectedOutput()
    {
        Assert.Equal("\b", CompileOutput("++[>++[>++<-]<-]>>."));
    }

    [Fact]
    public void CompilerPreservesByteWraparoundAndRepeatedOutput()
    {
        var source = new string('+', 321) + "..--.";

        Assert.Equal("AA?", CompileOutput(source));
    }

    private static string CompileOutput(string source)
    {
        var ast = new Parser(new Lexer(source).lex()).ast();
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

        return output.ToString();
    }
}
