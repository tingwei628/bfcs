# Compiler tests

Run the compiler unit tests with:

```sh
dotnet test Compiler.Tests/Compiler.Tests.csproj
```

## Byte array opcodes

`ByteArrayOpcodeTests` runs the CIL sequences discussed in issue #7 for all
256 byte values. The sequences have the same read or write result for valid
indices of a `byte[]`.

- `ldelem.u1` reads from an array and index. `ldelema uint8` followed by
  `ldobj uint8` also reads the byte as a zero-extended 32-bit value.
- `stelem.i1` takes an array, index, and value. `stind.i1` and `stobj uint8`
  take an address and value. For an array element, obtain the address with
  `ldelema uint8` before loading the value to store.
