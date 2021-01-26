using System;

namespace NetGenRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            NetGenRunner runner = new NetGenRunner();
            args = args.Length == 0 ? new string[] { "3", "12" } : args;
            runner.Run(args);
        }
    }
}
