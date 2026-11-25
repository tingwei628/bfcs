using System;
using System.IO;
using Xunit;

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
    public void CompilerParsesNestedLoopsAndEmitsExpectedOutput()
    {
        var ast = new Parser(new Lexer("++[>++[>++<-]<-]>>.").lex()).ast();
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

        Assert.Equal(((char)8).ToString(), output.ToString());
    }
}
