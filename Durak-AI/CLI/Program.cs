using System;

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            CLAP.Parser.Run<Parser>(args);
        }
    }
}