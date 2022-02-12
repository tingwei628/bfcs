# bfcs 

> Brainfuck compiler/interpreter in C#


## Brainfuck Compiler in C#

> Target: CIL

Build the compiler "BFC" in `release/`
```
cd Compiler
make release
```

Compile abc.bf and execute
```
./release/BFC abc.bf
```

## Brainfuck Interpreter in C#

Build the interpreter "bfi"
```
cd Interpreter
make
```

Interpret abc.bf
```
make run bf=./abc.bf
```


### Credits in cil code generation
[Brainfuck.NET @nikeee](https://github.com/nikeee/Brainfuck.NET)


### Reference

[brainfuck visualizer@fatiherikli](http://fatiherikli.github.io/brainfuck-visualizer)

[other brainfuck implementations](https://esolangs.org/wiki/Brainfuck_implementations)

[JITBrainfuck@JLChnToZ](https://github.com/JLChnToZ/JITBrainfuck)
