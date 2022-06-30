using System;
using System.IO;

namespace Helpers.Writer
{
    public class Writer : IWriter
    {
        private readonly bool verbose;
        private readonly TextWriter writer;

        private ConsoleColor[] colors = { 
            ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.Blue, ConsoleColor.White
        };

        public Writer(TextWriter writer, bool verbose)
        {
            this.writer = writer;
            this.verbose = verbose;
        }

        public void Write(string s)
        {
            Console.ForegroundColor = colors[3];
            writer.Write(s);
        }

        public void WriteLine()
        {
            writer.WriteLine();
        }

        public void WriteLine(string s)
        {
            Console.ForegroundColor = colors[3];
            writer.WriteLine(s);
        }

        public void WriteVerbose(string s)
        {
            if (verbose)
            {
                Console.ForegroundColor = colors[3];
                writer.Write(s);
            }
        }

        public void WriteLineVerbose()
        {
            if (verbose)
            {
                writer.WriteLine();
            }
        }

        public void WriteLineVerbose(string s)
        {
            if (verbose)
            {
                Console.ForegroundColor = colors[3];
                writer.WriteLine(s);
            }
        }


        public void WriteVerbose(string text, int id)
        {
            if (verbose)
            {
                Console.ForegroundColor = colors[id];
                writer.Write(text);
            }
        }

        public void WriteLineVerbose(string text, int id)
        {
            if (verbose)
            {
                Console.ForegroundColor = colors[id];
                writer.WriteLine(text);
            }
        }
    }
}