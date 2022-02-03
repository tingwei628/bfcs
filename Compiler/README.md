## Brainfuck Compiler in C#

Target: CIL


- Lexer(token)
- Parser(syntax, AST)
- Code generator (emit IL)
- Optimizer

## Compilation and execution

Build the compiler "BFC" in `bin/publish/`
```
make release
```

Compile abc.bf and execute
```
./BFC abc.bf
```


### Credits in code generation
[Brainfuck.NET @nikeee](https://github.com/nikeee/Brainfuck.NET)
