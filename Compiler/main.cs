using System;
/*
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
    new BFC(args).compile();
  }
}
