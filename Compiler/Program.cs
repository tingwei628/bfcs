using System;

namespace BFC
{
    class Program
    {
        static void Main(string[] args)
        {
            new BrainFuckCompiler(args).compile();
        }
    }
}
