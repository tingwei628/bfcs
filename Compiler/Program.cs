using System;

namespace BFC
{
    class Program
    {
        static void Main(string[] args)
        {
#if LLVM
            new BrainFuckCompiler(args).compile();
#elif BENCHTEST
            new BrainFuckCompiler(args).compile();
#else // CIL
            new BrainFuckCompiler(args).compile();
#endif
            
        }
    }
}
