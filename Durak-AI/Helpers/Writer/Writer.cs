using System;
using System.IO;

namespace Helpers.Writer
{
    public class Writer : IWriter
    {
        private readonly bool verbose;
        private readonly TextWriter writer;

        public Writer(TextWriter writer, bool verbose)
        {
            this.writer = writer;
            this.verbose = verbose;
        }

        public void Write(string s)
        {
            SetDefaultColor();
            writer.Write(s);
        }

        public void WriteLine()
        {
            writer.WriteLine();
        }

        public void WriteLine(string s)
        {
            SetDefaultColor();
            writer.WriteLine(s);
        }

        public void WriteVerbose(string s)
        {
            if (verbose)
            {
                SetDefaultColor();
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
                SetDefaultColor();
                writer.WriteLine(s);
            }
        }

        /*
            These methods will display the output with different colors based on
            id param. Color.Green is assigned to 0. Color.Red to 1. 
         */
        public void SetColor(int id)
        {
            if (id == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                return;
            }
            Console.ForegroundColor = ConsoleColor.Red;
        }


        public void WriteVerbose(string text, int id)
        {
            if (verbose)
            {
                SetColor(id);
                writer.Write(text);
            }
        }

        public void WriteLineVerbose(string text, int id)
        {
            if (verbose)
            {
                SetColor(id);
                writer.WriteLine(text);
            }
        }

        /*
         This method sets the color to default
         */
        public void SetDefaultColor()
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}