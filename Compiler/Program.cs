using System;

namespace BFC
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--emit-llvm")
            {
                if (args.Length != 2)
                {
                    Console.Error.WriteLine("Usage: BFC --emit-llvm <source.bf>");
                    return 2;
                }

                var program = BrainFuckCompiler.ParseFile(args[1]);
                LlvmCodeGenerator.Generate(program, Console.Out);
                return 0;
            }

            new BrainFuckCompiler(args).compile();
            return 0;
        }
    }
}
