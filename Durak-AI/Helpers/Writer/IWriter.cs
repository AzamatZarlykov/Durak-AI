﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.Writer
{
    public interface IWriter
    {
        void Write(string s);
        void WriteVerbose(string s);
        void WriteLine();
        void WriteLine(string text);
        void WriteLineVerbose();
        void WriteLineVerbose(string text);
    }
}