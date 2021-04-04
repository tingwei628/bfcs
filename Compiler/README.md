## Brainfuck Compiler in C#

Target: CIL


- Lexer(token)
- Parser(syntax, AST)
- ~~Semantic Analyzer (type checking, Label checking and Flow control checking.)~~
- Code generator (emit IL)
- Optimizer



### Repl.it

compile bfc.cs
```
mcs -out:bfc bfc.cs main.cs

```

compile with test.bf to generate cil code (bfAsm.exe) 
```
mono bfc ./test.bf

```

execute bfAsm.exe to see bf result
```
mono bfAsm.exe
```

view cil code
```
monodis bfAsm.exe
```

### Credits in code generation
[Brainfuck.NET @nikeee](https://github.com/nikeee/Brainfuck.NET)
