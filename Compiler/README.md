## Brainfuck Compiler in C#

Target: CIL


- Lexer(token)
- Parser(syntax, AST)
- Semantic Analyzer (type checking, Label checking and Flow control checking.)
- Code generator (emit IL)
- Optimizer



### Repl.it

compile bfc.cs
```
mcs bfc.cs

```

compile test.bf to CIL
```
mono bfc.exe ./test.bf

```
