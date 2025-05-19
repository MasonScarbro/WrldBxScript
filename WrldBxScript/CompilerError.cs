using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    class CompilerError : Exception
    {
        public readonly Token token;

        public CompilerError(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
}
