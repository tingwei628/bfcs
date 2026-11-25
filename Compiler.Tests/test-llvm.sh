#!/bin/sh
set -eu

if [ "$#" -eq 0 ]; then
    echo "Usage: sh Compiler.Tests/test-llvm.sh <BFC command...>" >&2
    exit 2
fi

scratch=$(mktemp -d)
trap 'rm -rf "$scratch"' EXIT
repo=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)

check_program() {
    source=$1
    shift
    "$@" --emit-llvm "$source" > "$scratch/program.ll"
    clang -O2 -Wno-override-module "$scratch/program.ll" -o "$scratch/program"
    "$scratch/program" < "$scratch/input" > "$scratch/actual"
    cmp "$scratch/expected" "$scratch/actual"
}

: > "$scratch/input"
printf 'Hello World!\n' > "$scratch/expected"
check_program "$repo/Compiler/test/hello_world.bf" "$@"

# Transfer the remaining count 50 cells to the right on each iteration.
# This travels from cell 1 to cell 6251, then reads back cell 0.
awk 'BEGIN {
    for (i = 0; i < 66; i++) printf "+"
    printf ">"
    for (i = 0; i < 125; i++) printf "+"
    printf "[[-"
    for (i = 0; i < 50; i++) printf ">"
    printf "+"
    for (i = 0; i < 50; i++) printf "<"
    printf "]"
    for (i = 0; i < 50; i++) printf ">"
    printf "-]"
    for (i = 0; i < 65; i++) printf "+"
    printf "."
    for (i = 0; i < 6251; i++) printf "<"
    printf "."
}' > "$scratch/tape.bf"
printf 'AB' > "$scratch/expected"
check_program "$scratch/tape.bf" "$@"

printf ',.+.,.' > "$scratch/io.bf"
printf '\377' > "$scratch/input"
printf '\377\000\377' > "$scratch/expected"
check_program "$scratch/io.bf" "$@"

: > "$scratch/empty.bf"
: > "$scratch/expected"
check_program "$scratch/empty.bf" "$@"

printf '[[]' > "$scratch/invalid.bf"
if "$@" --emit-llvm "$scratch/invalid.bf" > "$scratch/invalid.ll" 2> "$scratch/error"; then
    echo "Expected unmatched brackets to fail compilation" >&2
    exit 1
fi
test ! -s "$scratch/invalid.ll"
grep -F 'no matched ]' "$scratch/error" > /dev/null
printf 'LLVM integration checks passed\n'
