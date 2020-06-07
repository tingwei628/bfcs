using System;
using System.Collections.Generic;
using System.IO;
/*

    Reading the source code file
    The Lexical Analyzer which translates the source file to a stream of tokens
    The Syntax Analyzer which builds an Abstract Syntax Tree (AST) from the stream of tokens and checks simple things like if loops match
    The Intermediate Code Generator which takes the Source AST and transforms it into an Intermediate AST
    The Target Code Generator which takes the Intermediate AST and transforms it into a Target AST
    The Target Code Writer which takes the Target AST and writes actual x86 Assembly code as output.
 
    https://cs.lmu.edu/~ray/notes/ohmexamples/

   
    Lexer(token)
    Parser(syntax, AST, LL parser) ? Validate? [] should be in pair
    Semantic Analyzer (type checking, Label checking and Flow control checking.)
    Code generator (emit IL)
    Optimizer (JIT...)

    Parser : https://www.cs.fsu.edu/~engelen/courses/COP402003/board.html#production
    
    System.Reflection.Emit
    ILGenerator

*/
public class BFC {
  public string[] _args { get; }
  public BFC(string[] args) {
    this._args = args;
  }
  public void compile() {
    if (this._args.Length < 1) throw new ArgumentNullException("no bf file");
    string bf_filepath = this._args[0];
    string str = File.ReadAllText(bf_filepath);
    var tokens = new Lexer(str).lex();
    var ast = new Parser(tokens).ast();
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
  public char Literal { get; }
  public Token_Enum TokenType { get; }
  public int _pos;
  public bool IsScan { get; set; }
  public Token(char literal, Token_Enum token_Enum, int pos) {
    TokenType = token_Enum;
    Literal = literal;
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
        default: break; // ignore Error token
      }
      i++;
    }
    return tokens;
  }
}

/*
	
Program → Instr Program | ε

Instr → '+' | '-' | '>' | '<' | ',' | '.' | '[' Program ']'

LL(1)

*/
public class ASTNode {
  public ASTNode LeftNode { get; set; }
  public ASTNode MiddleNode { get; set; }
  public ASTNode RightNode { get; set; }
  public char Value { get; }
  public ASTNode() {
  }
  public ASTNode(Token token) {
    Value = token.Literal;
  }
}
public class Parser {
  private List<Token>_tokens = new List<Token>();
  private int _currentIndex;
  private int _endIndex;
  public Parser(List<Token> tokens) {
    _tokens = tokens;
    _currentIndex = 0;
    _endIndex = tokens.Count == 0 ? -1 : tokens.Count-1;
  }
  public ASTNode ast() {
    ASTNode ast = _program();
    return ast;
  }
  // [+]
  private ASTNode _program() {
    if (_endIndex == -1)  return null;
    if (_isNextTokenEOF()) return null;
    
    
    ASTNode leftNode = _instr();
    if (leftNode == null) return null;

    ASTNode rightNode = _program();
    if (rightNode == null) return leftNode;
    
    ASTNode ast = new ASTNode();
    ast.LeftNode = leftNode;
    ast.RightNode = rightNode;
    
    return ast;
  }
  private ASTNode _instr() {
    Token token = _getCurrentToken();
    
    if (token == null) return null;
    if (token.IsScan == true) return null;

    if (isTerminals(token.Literal)){
      //token.IsScan = true;
      return new ASTNode(token);
    }
    else if (token.TokenType == Token_Enum.BeginLoop) {
      int searchIndex_right = _currentIndex;
      int bracket_right = 1;
      while(bracket_right > 0) {
        if (_tokens[searchIndex_right].TokenType == Token_Enum.EndLoop) bracket_right--;
        else if (_tokens[searchIndex_right].TokenType == Token_Enum.BeginLoop) bracket_right++;
        if (_endIndex < searchIndex_right) break;
        searchIndex_right++;
      }
      if (bracket_right > 0 ) {
        throw new Exception("no found ]");
      }
      
      ASTNode ast = new ASTNode();
      ast.LeftNode = new ASTNode(token);
      ASTNode middleNode = _program();
      if (middleNode != null)  ast.MiddleNode = middleNode;
      ast.RightNode = new ASTNode(_tokens[searchIndex_right]);
      token.IsScan = true;
      _tokens[searchIndex_right].IsScan = true;

      return ast;
    }
    //extra ]
    throw new Exception("unkown error");
    return null;
  }
  private bool isTerminals(char literal) {
    return literal == '+' || literal == '-' || literal == '>' || literal == '<' || literal == ',' || literal == '.';
  }
  private Token _getCurrentToken() {
    if (_endIndex < _currentIndex) return null;
    return _tokens[_currentIndex++];
  }
  private bool _isNextTokenEOF () {
    return _endIndex < _currentIndex;
  }
}
public class SemanticAnalyzer {

}

public class CodeGenerator {
  public string gen() {
    return null;
  }
}
