using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI.exceptions
{
    public class UnknownAIException : Exception
    {
        public UnknownAIException()
        {
        }

        public UnknownAIException(string message)
            : base(message)
        {
        }

        public UnknownAIException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
