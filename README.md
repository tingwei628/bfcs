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


## Loops and tail calls

The Brainfuck grammar has eight commands and provides no functions or calls.
The CIL backend emits loop conditions and branch targets directly. A closing
bracket branches back within the same generated method, so loop iterations
use constant call-stack space. The CIL `tail.` prefix applies to a method call
in tail position and has no corresponding operation in this grammar.

### Credits in cil code generation
[Brainfuck.NET @nikeee](https://github.com/nikeee/Brainfuck.NET)


### Reference

[brainfuck visualizer@fatiherikli](http://fatiherikli.github.io/brainfuck-visualizer)

[other brainfuck implementations](https://esolangs.org/wiki/Brainfuck_implementations)

[JITBrainfuck@JLChnToZ](https://github.com/JLChnToZ/JITBrainfuck)
