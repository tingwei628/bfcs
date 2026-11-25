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

### Bootstrapping a Brainfuck compiler

To self-host, implement a compiler in Brainfuck that reads Brainfuck source
from standard input and emits a compilable representation to standard output.
Use a text format such as C so the compiler can emit it with ordinary byte I/O.
For the first version, translate the eight commands directly and validate
bracket pairing. Use the same input termination convention in both stages.

Once you have that implementation in `compiler.bf`, use BFC as the seed.
The following example assumes the Brainfuck compiler emits C.

```sh
BFC=./Compiler/release/BFC
"$BFC" compiler.bf < compiler.bf > stage1.c
cc stage1.c -o stage1
./stage1 < compiler.bf > stage2.c
cc stage2.c -o stage2
./stage2 < compiler.bf > stage3.c
cmp stage2.c stage3.c
./stage2 < Compiler/test/hello_world.bf > hello.c
cc hello.c -o hello
./hello
```

The comparison checks that the compiler's output has stabilized for its own
source. Running the generated example also checks that the result can compile
a separate program. This repository supplies the C# seed compiler. The
`compiler.bf` implementation is a separate program you provide.

### Loops and tail calls

The Brainfuck grammar has eight commands and provides no functions or calls.
The CIL backend emits loop conditions and branch targets directly. A closing
bracket branches back within the same generated method, so loop iterations
use constant call-stack space. The CIL `tail.` prefix applies to a method call
in tail position and has no corresponding operation in this grammar.

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
