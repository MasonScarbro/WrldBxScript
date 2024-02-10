using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    class Parser
    {
        private class ParseError : FormatException { }

        private readonly List<Token> tokens;
        private int current = 0;
        private int loopDepth = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        /*
         * When KickStarting the Descent We will probably only need a form of VarDeclaration in order to 
         * parse KEYWORD: EXPR
         * Later we Might have to handle it being assigned for other various functions of the scripting lang
         * but for now the assignment can only be a string or a number (expr counts too)
         * for code generation we could have a check for each time a "var" is declared we check for what 
         * keyword it was associated with in order to prepend the proper code 
         */
        public List<Stmt> Parse()
        {
            List<Stmt> stmts = new List<Stmt>();
            while (!IsAtEnd())
            {
                stmts.Add(PLACEHOLDER()); // PLACEHOLDER is just so I can upload this with no ERRORS 

            }

            return stmts;
        }

        private Stmt PLACEHOLDER() 
        {
            return new Stmt.Var(Previous(), null);
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            //else
            return Peek().GetTokenType() == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().type == TokenType.EOF;
        }

        private Token Peek()
        {
            return tokens[current];

        }

        private Token Previous()
        {
            return tokens[current - 1];
        }


    }
}
