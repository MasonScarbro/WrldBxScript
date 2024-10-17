using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public struct Token
    {

        public readonly TokenType type;
        public readonly String lexeme;
        public readonly object literal;
        public readonly int line;

        public Token (TokenType type, string lexeme, object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public string toString()
        {
            return type + " " + lexeme + " " + literal;
        }

        public TokenType GetTokenType()
        {
            return this.type;
        }

    }
}
