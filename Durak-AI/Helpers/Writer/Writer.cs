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
            writer.Write(s);
        }

        public void WriteLine()
        {
            writer.WriteLine();
        }

        public void WriteLine(string s)
        {
            writer.WriteLine(s);
        }

        public void WriteVerbose(string s)
        {
            if (verbose)
            {
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
                writer.WriteLine(s);
            }
        }
    }
}