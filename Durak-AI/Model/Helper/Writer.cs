using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Helpers
{
    public class Writer
    {
        private readonly bool verbose;
        private readonly bool debug;
        private readonly TextWriter writer;

        private ConsoleColor[] colors = { 
            ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.Blue, ConsoleColor.White
        };

        public Writer(TextWriter writer, bool verbose, bool debug)
        {
            this.writer = writer;
            this.verbose = verbose;
            this.debug = debug;
        }

        ~Writer()
        {
            // change to Console.Out default color
            Console.ForegroundColor = colors[3];
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
        
        public void WriteVerbose(string text, bool isCopy = false)
        {
            if (isCopy && debug)
            {
                Console.ForegroundColor = colors[3];
                writer.Write($"\t{text}");
            }

            else if (!isCopy && verbose)
            {
                Console.ForegroundColor = colors[3];
                writer.Write(text);
            }
        }

        public void WriteLineVerbose(bool isCopy = false)
        {
            if ((isCopy && debug) || (!isCopy && verbose))
            {
                writer.WriteLine();
            }
        }

        public void WriteLineVerbose(string text, bool isCopy = false)
        {
            if (isCopy && debug)
            {
                Console.ForegroundColor = colors[3];
                writer.WriteLine($"\t{text}");
            }

            else if (!isCopy && verbose)
            {
                Console.ForegroundColor = colors[3];
                writer.WriteLine(text);
            }
        }


        public void WriteVerbose(string text, int id, bool isCopy = false, bool isCards = false)
        {
            if (isCopy && debug)
            {
                Console.ForegroundColor = colors[id];
                writer.Write(isCards ? text : $"\t{text}");
            }

            else if (!isCopy && verbose)
            {
                Console.ForegroundColor = colors[id];
                writer.Write(text);
            }
        }

        public void WriteLineVerbose(string text, int id, bool isCopy = false, bool isCards = false)
        {
            if (isCopy && debug)
            {
                Console.ForegroundColor = colors[id];
                writer.WriteLine(isCards ? text : $"\t{text}");
            }

            else if (!isCopy && verbose)
            {
                Console.ForegroundColor = colors[id];
                writer.WriteLine(text);
            }
        }
    }
}
