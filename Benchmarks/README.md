# Performance probes

Run from the repository root with Python 3.10+ and the .NET 8 SDK. The
existing compiler targets .NET Core 3.1. If only a newer runtime is installed,
set `DOTNET_ROLL_FORWARD=Major` for this run. Clang 15+ on a 64-bit host
enables the LLVM cases. The runner uses Clang from `PATH` or the executable
provided with `--clang`.

```sh
DOTNET_ROLL_FORWARD=Major python3 Benchmarks/run.py --samples 5
```

The runner builds Release binaries and generates fixtures in a temporary
directory. It checks expected
output before timing, reports median and range, and exits nonzero for a
mismatch, timeout, or process failure. Rerun on comparable machines and
runtimes when evaluating a change.

| Probe | What it measures |
| --- | --- |
| Compiler front end | Time and current-thread managed allocation for lexing and parsing after a small warmup. File reading is outside the measurement. Identical instruction counts are arranged as a single run or alternating instructions to vary the size of the AST. |
| CIL phases | IL generation (including dynamic type creation), native code preparation via `RuntimeHelpers.PrepareMethod`, and one invocation of the prepared method. The probe warms up with a separate method and disables tiered compilation. |
| CIL flat inputs | End-to-end process time for those same inputs, including startup, parsing, IL generation, JIT, and execution. |
| Cell clearing | 62,500 resets starting with a cell value of 255. The Brainfuck control writes the equivalent result with a single increment from 255. |
| Output | The same nested-loop structure with one write or 1,000,001 writes. The timed output goes to the null device, so terminal rendering is outside the measurement. |

CIL end-to-end timings include process startup and use the default runtime settings. The separate phase medians describe a warmed harness with tiering disabled and are not expected to add up to the end-to-end median. LLVM timings start from
a native binary compiled with Clang `-O2`, so IR generation and linking are
outside the measurement. Each pair is intended for comparison within its
own backend. The controls can also change the optimizer's ability to
eliminate work.

## Initial baseline

Five samples per case on Linux x86-64, AMD EPYC 9V74, .NET SDK 8.0.428,
and Clang 19.1.7. Values are median wall time in milliseconds unless noted.

| Probe | Baseline comparison |
| --- | --- |
| Front end, 32,000 grouped → alternating instructions | 0.95 → 3.55 ms and 1,292,800 → 4,364,704 allocated bytes |
| CIL emission and type creation, same inputs | 0.20 → 89.44 ms |
| CIL JIT preparation, same inputs | 0.09 → 160.56 ms |
| CIL prepared invocation, same inputs | 0.01 → 0.20 ms |
| CIL end to end, same inputs | 55.8 → 293.3 ms |
| Cell clearing, direct → loop | CIL 60.8 → 97.4 ms, LLVM 0.88 → 42.0 ms |
| Output, one → 1,000,001 writes | CIL 63.6 → 270.3 ms, LLVM 1.29 → 5.71 ms |

JIT preparation was the largest measured phase for the alternating input,
followed by IL generation. Reducing emitted IL for mixed arithmetic runs is
a candidate to measure before optimizing parser allocation. The 32,000-instruction
cases completed, so this run did not establish a stack limit. The cell-clearing
and output probes justify testing candidate optimizations on representative
programs if those workloads matter. These figures describe synthetic fixtures
and one machine. They are reference measurements without CI thresholds.
