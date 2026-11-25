using System;
using System.Reflection.Emit;
using Xunit;

public class ByteArrayOpcodeTests
{
    [Fact]
    public void LoadOpcodesReturnTheSameUnsignedValueForEveryByte()
    {
        var directLoad = CreateLoad(useAddress: false);
        var addressLoad = CreateLoad(useAddress: true);
        var tape = new byte[2];

        for (int value = byte.MinValue; value <= byte.MaxValue; value++)
        {
            tape[1] = (byte)value;

            Assert.Equal(value, directLoad(tape, 1));
            Assert.Equal(value, addressLoad(tape, 1));
        }
    }

    [Fact]
    public void StoreOpcodesWriteTheSameByteWithoutChangingAdjacentCells()
    {
        var directStore = CreateStore(OpCodes.Stelem_I1);
        var indirectStore = CreateStore(OpCodes.Stind_I1);
        var objectStore = CreateStore(OpCodes.Stobj);
        var tape = new byte[] { 42, 0, 99 };

        for (int value = byte.MinValue; value <= byte.MaxValue; value++)
        {
            var expected = new byte[] { 42, (byte)value, 99 };
            foreach (var store in new[] { directStore, indirectStore, objectStore })
            {
                tape[1] = (byte)~value;
                store(tape, 1, (byte)value);

                Assert.Equal(expected, tape);
            }
        }
    }

    private static Func<byte[], int, int> CreateLoad(bool useAddress)
    {
        var method = new DynamicMethod("LoadByte", typeof(int), new[] { typeof(byte[]), typeof(int) });
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);

        if (useAddress)
        {
            il.Emit(OpCodes.Ldelema, typeof(byte));
            il.Emit(OpCodes.Ldobj, typeof(byte));
        }
        else
        {
            il.Emit(OpCodes.Ldelem_U1);
        }

        il.Emit(OpCodes.Ret);
        return (Func<byte[], int, int>)method.CreateDelegate(typeof(Func<byte[], int, int>));
    }

    private static Action<byte[], int, byte> CreateStore(OpCode storeOpcode)
    {
        var method = new DynamicMethod("StoreByte", typeof(void), new[] { typeof(byte[]), typeof(int), typeof(byte) });
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);

        if (storeOpcode != OpCodes.Stelem_I1)
        {
            il.Emit(OpCodes.Ldelema, typeof(byte));
        }

        il.Emit(OpCodes.Ldarg_2);
        if (storeOpcode == OpCodes.Stobj)
        {
            il.Emit(storeOpcode, typeof(byte));
        }
        else
        {
            il.Emit(storeOpcode);
        }

        il.Emit(OpCodes.Ret);
        return (Action<byte[], int, byte>)method.CreateDelegate(typeof(Action<byte[], int, byte>));
    }
}
