## Brainfuck Compiler in C#

Target: CIL


- Lexer(token)
- Parser(syntax, AST)
- Code generator (emit IL)
- Optimizer



### Repl.it

compile with hello_world.bf to generate cil code (bfAsm.exe) 
```
make compile bf=hello_world.bf

```

execute bfAsm.exe to see bf result
```
make run
```

view cil code
```
monodis bfAsm.exe
```

### Credits in code generation
[Brainfuck.NET @nikeee](https://github.com/nikeee/Brainfuck.NET)
