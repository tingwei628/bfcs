#!/usr/bin/env python3
"""Run bfcs performance probes with no third-party Python packages."""

import argparse
import shutil
import statistics
import subprocess
import tempfile
import time
from pathlib import Path


def build_probe(root, work, name, files):
    project = work / name
    project.mkdir()
    for source in files:
        shutil.copyfile(root / source, project / Path(source).name)
    (project / f"{name}.csproj").write_text(
        '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>'
        '<OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework>'
        '</PropertyGroup></Project>\n',
        encoding="utf-8",
    )
    return build(project / f"{name}.csproj", name, "net8.0")


def build(project, name, framework):
    subprocess.run(["dotnet", "build", str(project), "-c", "Release",
                    "-p:UseSharedCompilation=false", "--nologo", "-v:q"], check=True)
    return project.parent / "bin" / "Release" / framework / f"{name}.dll"


def nested(body):
    counter = "+" * 250
    return counter + "[>" + counter + "[>" + body + "<-]<-]"


def bracket_workload(span):
    counter = "+" * 200
    return counter + "[>" + counter + "[>[" + "-" * span + "]<-]<-]" + "+" * 65 + "."


def run(command, timeout, output):
    return subprocess.run(command, stdin=subprocess.DEVNULL, stdout=output,
                          stderr=subprocess.PIPE, timeout=timeout, check=True)


def measure(label, command, args, expected=None, frontend=False):
    try:
        result = run(command, args.timeout, subprocess.PIPE)
        if expected is not None and result.stdout != expected:
            print(f"{label:32} INVALID OUTPUT ({len(result.stdout)} bytes)")
            return False

        elapsed = []
        allocations = []
        for _ in range(args.samples):
            start = time.perf_counter()
            result = run(command, args.timeout,
                         subprocess.PIPE if frontend else subprocess.DEVNULL)
            wall_ms = (time.perf_counter() - start) * 1000
            if frontend:
                milliseconds, count = result.stdout.split()
                elapsed.append(float(milliseconds))
                allocations.append(int(count))
            else:
                elapsed.append(wall_ms)
    except subprocess.TimeoutExpired:
        print(f"{label:32} TIMEOUT ({args.timeout:g}s)")
        return False
    except subprocess.CalledProcessError as error:
        lines = error.stderr.decode(errors="replace").splitlines()
        detail = lines[0] if lines else "no diagnostic"
        print(f"{label:32} ERROR ({error.returncode}): {detail}")
        return False

    allocation = f", {statistics.median(allocations):,.0f} allocated bytes" if frontend else ""
    print(f"{label:32} {statistics.median(elapsed):9.3f} ms "
          f"[{min(elapsed):.3f}, {max(elapsed):.3f}]{allocation}")
    return True


def benchmark(args, root, work):
    compiler = work / "Compiler"
    shutil.copytree(root / "Compiler", compiler, ignore=shutil.ignore_patterns("bin", "obj", "release"))
    cil = ["dotnet", str(build(compiler / "BFC.csproj", "BFC", "netcoreapp3.1"))]
    interpreter = ["dotnet", str(build_probe(root, work, "Interpreter", ["Interpreter/bfi.cs"]))]
    frontend = ["dotnet", str(build_probe(root, work, "Frontend",
                                         ["Benchmarks/Frontend.cs", "Compiler/bfc.cs"]))]

    def source(name, program):
        path = work / f"{name}.bf"
        path.write_text(program, encoding="ascii")
        return str(path)

    print(f"\n{args.samples} samples per case, median [min, max]\n", flush=True)
    failed = False
    for span in (16, 4096):
        path = source(f"brackets-{span}", bracket_workload(span))
        failed |= not measure(f"interpreter brackets {span}", [*interpreter, path], args, b"A")

    for count in (1000, 8000, 32000):
        for name, program in (("grouped", "+" * count), ("alternating", "+-" * (count // 2))):
            path = source(f"{name}-{count}", program)
            failed |= not measure(f"frontend {name} {count}", [*frontend, path], args, frontend=True)
            failed |= not measure(f"CIL {name} {count}", [*cil, path], args, b"")

    scenarios = (
        ("clear-loop", nested("-[-]") + ">>" + "+" * 65 + ".", b"A"),
        ("clear-direct", nested("-+") + ">>" + "+" * 65 + ".", b"A"),
        ("output-control", ">>" + "+" * 65 + "<<" + nested("") + ">>.", b"A"),
        ("output-heavy", ">>" + "+" * 65 + "<<" + nested("." * 16) + ">>.", b"A" * 1000001),
    )
    for name, program, expected in scenarios:
        path = source(name, program)
        failed |= not measure(f"CIL {name}", [*cil, path], args, expected)
        if args.clang:
            ir = work / f"{name}.ll"
            native = work / f"{name}.exe"
            with ir.open("wb") as output:
                run([*cil, "--emit-llvm", path], args.timeout, output)
            subprocess.run([args.clang, "-O2", "-Wno-override-module", str(ir),
                            "-o", str(native)], check=True, timeout=args.timeout)
            failed |= not measure(f"LLVM {name}", [str(native)], args, expected)
    return int(failed)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--samples", type=int, default=5)
    parser.add_argument("--timeout", type=float, default=20, help="per-run timeout in seconds")
    parser.add_argument("--clang", default=shutil.which("clang"),
                        help="Clang path for LLVM execution probes")
    args = parser.parse_args()
    if args.samples < 1 or args.timeout <= 0:
        parser.error("samples and timeout must be positive")
    if not args.clang:
        print("Clang unavailable. Running the managed probes only.", flush=True)
    root = Path(__file__).resolve().parent.parent
    with tempfile.TemporaryDirectory(prefix="bfcs-bench-") as temporary:
        return benchmark(args, root, Path(temporary))


if __name__ == "__main__":
    raise SystemExit(main())
