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
| CIL flat inputs | End-to-end process time for those same inputs, including startup, parsing, IL generation, JIT, and execution. |
| Cell clearing | 62,500 resets starting with a cell value of 255. The Brainfuck control writes the equivalent result with a single increment from 255. |
| Output | The same nested-loop structure with one write or 1,000,001 writes. The timed output goes to the null device, so terminal rendering is outside the measurement. |

CIL timings include process startup. LLVM timings start from
a native binary compiled with Clang `-O2`, so IR generation and linking are
outside the measurement. Each pair is intended for comparison within its
own backend. The controls can also change the optimizer's ability to
eliminate work.

## Initial baseline

Five samples per case on Linux x86-64, AMD EPYC 9V74, .NET SDK 8.0.428,
and Clang 19.1.7. Values are median wall time in milliseconds unless noted.

| Probe | Baseline comparison |
| --- | --- |
| Front end, 32,000 grouped → alternating instructions | 2.78 → 6.27 ms and 1,292,800 → 4,364,704 allocated bytes |
| CIL, 32,000 grouped → alternating instructions | 45.9 → 278.6 ms |
| Cell clearing, direct → loop | CIL 54.8 → 87.3 ms, LLVM 0.85 → 35.4 ms |
| Output, one → 1,000,001 writes | CIL 60.9 → 220.2 ms, LLVM 0.92 → 6.29 ms |

For CIL, the difference between front-end and end-to-end timings motivates profiling
IL generation and JIT cost before changing the parser. The 32,000-instruction
cases completed, so this run did not establish a stack limit. The cell-clearing
and output probes justify testing candidate optimizations on representative
programs if those workloads matter. These figures describe synthetic fixtures
and one machine. They are reference measurements without CI thresholds.
