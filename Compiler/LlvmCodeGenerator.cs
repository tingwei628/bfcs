using System;
using System.Collections.Generic;
using System.IO;

internal sealed class LlvmCodeGenerator
{
    // The generated program runs on a 64-bit target. Its tape grows on access,
    // and zeroing the new portion preserves Brainfuck's initial cell values.
    private const string Runtime = @"@tape = internal global ptr null
@capacity = internal global i64 0

declare ptr @realloc(ptr, i64)
declare void @free(ptr)
declare void @abort() noreturn
declare i32 @getchar()
declare i32 @putchar(i32)

define internal ptr @cell(i64 %position) {
entry:
  %invalid = icmp ugt i64 %position, 2147483646
  br i1 %invalid, label %fail, label %check
check:
  %old = load ptr, ptr @tape
  %size = load i64, ptr @capacity
  %fits = icmp ult i64 %position, %size
  br i1 %fits, label %ready, label %grow
grow:
  %required = add i64 %position, 1
  %double = mul i64 %size, 2
  %enough = icmp uge i64 %double, %required
  %wanted = select i1 %enough, i64 %double, i64 %required
  %over = icmp ugt i64 %wanted, 2147483647
  %next = select i1 %over, i64 2147483647, i64 %wanted
  %new = call ptr @realloc(ptr %old, i64 %next)
  %failed = icmp eq ptr %new, null
  br i1 %failed, label %fail, label %initialize
initialize:
  %added = sub i64 %next, %size
  %first = getelementptr i8, ptr %new, i64 %size
  call void @llvm.memset.p0.i64(ptr %first, i8 0, i64 %added, i1 false)
  store ptr %new, ptr @tape
  store i64 %next, ptr @capacity
  br label %ready
ready:
  %buffer = phi ptr [ %old, %check ], [ %new, %initialize ]
  %address = getelementptr i8, ptr %buffer, i64 %position
  ret ptr %address
fail:
  call void @abort()
  unreachable
}

declare void @llvm.memset.p0.i64(ptr, i8, i64, i1 immarg)
";

    private readonly TextWriter output;
    private readonly Stack<int> loops = new Stack<int>();
    private int nextId;

    private LlvmCodeGenerator(TextWriter output)
    {
        this.output = output;
    }

    public static void Generate(ASTNode program, TextWriter output)
    {
        output.WriteLine(Runtime);
        output.WriteLine("define i32 @main() {");
        output.WriteLine("entry:");
        output.WriteLine("  %position = alloca i64");
        output.WriteLine("  store i64 0, ptr %position");
        new LlvmCodeGenerator(output).EmitProgram(program);
        output.WriteLine("  %buffer = load ptr, ptr @tape");
        output.WriteLine("  call void @free(ptr %buffer)");
        output.WriteLine("  ret i32 0");
        output.WriteLine("}");
    }

    private void EmitProgram(ASTNode program)
    {
        var pending = new Stack<ASTNode>();
        pending.Push(program);
        while (pending.Count > 0)
        {
            var node = pending.Pop();
            if (node == null) continue;

            EmitInstruction(node);
            // Visit the opening bracket, loop body, and closing bracket in order.
            pending.Push(node.RightNode);
            pending.Push(node.MiddleNode);
            pending.Push(node.LeftNode);
        }
    }

    private void EmitInstruction(ASTNode node)
    {
        switch (node.TokenType)
        {
            case Token_Enum.Increment:
            case Token_Enum.Decrement:
                var address = CellAddress();
                var current = Value($"load i8, ptr {address}");
                var cellOperation = node.TokenType == Token_Enum.Increment ? "add" : "sub";
                var updated = Value($"{cellOperation} i8 {current}, {node.RepeatCount % 256}");
                Line($"store i8 {updated}, ptr {address}");
                break;
            case Token_Enum.MoveLeft:
            case Token_Enum.MoveRight:
                var position = Value($"load i64, ptr %position");
                var moveOperation = node.TokenType == Token_Enum.MoveRight ? "add" : "sub";
                var destination = Value($"{moveOperation} i64 {position}, {node.RepeatCount}");
                Line($"store i64 {destination}, ptr %position");
                break;
            case Token_Enum.Input:
                var inputAddress = CellAddress();
                var input = Value($"call i32 @getchar()");
                var inputByte = Value($"trunc i32 {input} to i8");
                Line($"store i8 {inputByte}, ptr {inputAddress}");
                break;
            case Token_Enum.Output:
                var cell = ReadCell();
                var character = Value($"zext i8 {cell} to i32");
                Line($"call i32 @putchar(i32 {character})");
                break;
            case Token_Enum.BeginLoop:
                var loop = nextId++;
                loops.Push(loop);
                Line($"br label %check{loop}");
                Line($"check{loop}:", indent: false);
                var condition = ReadCell();
                var isZero = Value($"icmp eq i8 {condition}, 0");
                Line($"br i1 {isZero}, label %end{loop}, label %body{loop}");
                Line($"body{loop}:", indent: false);
                break;
            case Token_Enum.EndLoop:
                var enclosingLoop = loops.Pop();
                Line($"br label %check{enclosingLoop}");
                Line($"end{enclosingLoop}:", indent: false);
                break;
        }
    }

    private string CellAddress()
    {
        var position = Value($"load i64, ptr %position");
        return Value($"call ptr @cell(i64 {position})");
    }

    private string ReadCell()
    {
        var address = CellAddress();
        return Value($"load i8, ptr {address}");
    }

    private string Value(FormattableString instruction)
    {
        var name = FormattableString.Invariant($"%v{nextId++}");
        Line($"{name} = {FormattableString.Invariant(instruction)}");
        return name;
    }

    private void Line(FormattableString line, bool indent = true)
    {
        if (indent) output.Write("  ");
        output.WriteLine(FormattableString.Invariant(line));
    }
}
