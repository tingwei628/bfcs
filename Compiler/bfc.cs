using System;
using System.Collections.Generic;
/*

    Reading the source code file
    The Lexical Analyzer which translates the source file to a stream of tokens
    The Syntax Analyzer which builds an Abstract Syntax Tree (AST) from the stream of tokens and checks simple things like if loops match
    The Intermediate Code Generator which takes the Source AST and transforms it into an Intermediate AST
    The Target Code Generator which takes the Intermediate AST and transforms it into a Target AST
    The Target Code Writer which takes the Target AST and writes actual x86 Assembly code as output.


   
    Lexer(token)
    Parser(syntax, AST) ? Validate?
    Semantic Analyzer (type checking, Label checking and Flow control checking.)
    Code generator (emit IL)
    Optimizer (JIT...)


    System.Reflection.Emit
    ILGenerator

*/
class MainClass {
  public static void Main (string[] args) {
    if (args.Length < 1) throw new ArgumentNullException("no bf file");
    string bf_filepath = args[0];
    string str = File.ReadAllText(bf_filepath);
    var result = new Lexer(str).lex();
    foreach (var r in result) {
      Console.WriteLine(r._literal);
      Console.WriteLine(r._token_Enum);
      Console.WriteLine(r._pos);
    }
  }
}

public enum Token_Enum {
  BeginLoop,/* [ */
  EndLoop, /* ]*/
  Input, /* , */
  Output,  /* .*/
  Increment, /* + */
  Decrement, /* - */
  MoveLeft,/*<*/
  MoveRight, /*>*/
}
public class Token {
  public char _literal;
  public Token_Enum _token_Enum;
  public int _pos;
  public Token(char literal, Token_Enum token_Enum, int pos) {
    _token_Enum = token_Enum;
    _literal = literal;
    _pos = pos;
  }
}
public class Lexer {
  private List<Token>tokens = new List<Token>();
  private string _str;
  public Lexer(string str) {
    this._str = str;
  }
  public List<Token> lex () {
    int len = this._str.Length;
    int i = 0;
    int j = 0;
    while(i < len) {
      switch(this._str[i]) {
        case '[': this.tokens.Add(new Token('[', Token_Enum.BeginLoop, j)); j++; break;
        case ']': this.tokens.Add(new Token(']', Token_Enum.EndLoop, j)); j++; break;
        case '+': this.tokens.Add(new Token('+', Token_Enum.Increment, j)); j++; break;
        case '-': this.tokens.Add(new Token('-', Token_Enum.Decrement, j)); j++; break;
        case ',': this.tokens.Add(new Token(',', Token_Enum.Input, j)); j++; break;
        case '.': this.tokens.Add(new Token('.', Token_Enum.Output, j)); j++; break;
        case '<': this.tokens.Add(new Token('<', Token_Enum.MoveLeft, j)); j++; break;
        case '>': this.tokens.Add(new Token('>', Token_Enum.MoveRight, j)); j++; break;
        default: break;
      }
      i++;
    }
    return tokens;
  }
}

public class Parser {
  private List<Token>_tokens = new List<Token>();
  public Parser(List<Token> tokens) {
    _tokens = tokens;
  }
  public List<Token> ast() {
    return null;
  }
}
public class SemanticAnalyzer {

}

public class CodeGenerator {
  public string gen() {
    return null;
  }
}
