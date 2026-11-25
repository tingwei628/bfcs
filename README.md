# bfcs 

> Brainfuck compiler/interpreter in C#


## Brainfuck Compiler in C#

> Targets: CIL and LLVM IR

Build the compiler "BFC" in `release/`
```
cd Compiler
make cil
```

Compile abc.bf and execute
```
./release/BFC abc.bf
```

### LLVM backend

From the repository root, emit LLVM IR and build a native executable with
Clang 15 or newer on a 64-bit target with a C standard library.

```sh
dotnet build Compiler/BFC.csproj
dotnet Compiler/bin/Debug/netcoreapp3.1/BFC.dll --emit-llvm Compiler/test/hello_world.bf > hello.ll
clang -O2 -Wno-override-module hello.ll -o hello
./hello
```

The native executable uses byte I/O and wraps cells modulo 256. End of input
stores 255. The tape grows when a program accesses a new cell on the right.
Accessing a negative cell exits with an error.

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
