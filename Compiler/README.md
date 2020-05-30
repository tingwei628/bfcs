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
mcs -out:main.exe bfc.cs main.cs

```

compile test.bf to CIL
```
mono main.exe ./test.bf

```
