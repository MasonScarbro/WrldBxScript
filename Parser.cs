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
                stmts.Add(Declaration()); // PLACEHOLDER is just so I can upload this with no ERRORS 

            }

            return stmts;
        }

        private Expr Expression()
        {
            return Term();
        }
        private Stmt Declaration()
        {
            if(Match(TokenType.TRAITS, TokenType.EFFECTS))
            {
                return Starter();
            }
            if (IsMinorKeyword())
            {
                return VarDeclaration();
            }
            //else
            return Satement();
        }

        private Stmt Starter()
        {
            Token type = Previous();
            Consume(TokenType.LEFT_BRACKET, "After a Starter (EFFECTS, TRAITS, Etc...) There Needs to Be a Block Stmt '['");
            List<Stmt> statements = new List<Stmt>();
            Console.WriteLine(Peek().lexeme);
            while (!Check(TokenType.RIGHT_BRACKET) && !IsAtEnd())
            {
                statements.Add(Declaration());

            }
            Consume(TokenType.RIGHT_BRACKET, "Expected a ']' after block");
            return new Stmt.Starter(type, statements);
        }
        private Stmt VarDeclaration()
        {
            Token type = Previous();
            Expr value = null;
            if (Match(TokenType.COLON))
            {
                
                value = Expression();

            }
            
            ConsumeEither(TokenType.COMMA, TokenType.RIGHT_BRACE, "Expected ',' after your variable declaration");
            return new Stmt.Var(type, value);
        }

        private Stmt Satement()
        {
            if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());
            //else
            return ExpressionStmt();
        }

        private List<Stmt> Block()
        {
            Console.WriteLine("Parsing Block");
            List<Stmt> statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());

            }
            Consume(TokenType.RIGHT_BRACE, "Expected a '}' after block");
            return statements;
        }

        private Stmt ExpressionStmt()
        {
            Expr expr = Expression();
            Consume(TokenType.COMMA, "Expeceted a ',' after value");
            return new Stmt.Expression(expr);
        }



        private Expr Term()
        {
            Expr expr = Primary();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token oper = Previous();
                Expr right = Primary();
                
                expr = new Expr.Binary(expr, oper, right);
            }
            return expr;
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER))
            {
                Console.WriteLine("Parsing Term " + Previous().lexeme);
                return new Expr.Literal(Previous().literal);
            }
            if (Match(TokenType.STRING))
            {
                return new Expr.Literal(Previous().lexeme);
            }

            throw new ParseError();

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

        private Token Consume(TokenType type, String message)
        {
            if (Check(type)) return Advance();
            //else
            throw Error(Peek(), message);
        }

        //Got to get this to work
        private Token ConsumeEither(TokenType type, TokenType type2, String message)
        {

            if (Check(type)) return Advance();
            // ending '}' syntax consume of variable declaration
            if ((Check(type2)))
            {
                current--;
                return Advance();
            }
            //else
            throw Error(Peek(), message);
        }

        private bool IsMinorKeyword()
        {
            return Match
                (
                    TokenType.DAMAGE, TokenType.HEALTH, TokenType.ATTACK_SPEED, 
                    TokenType.RANGE, TokenType.LOCALIZATION, TokenType.ID,
                    TokenType.DODGE, TokenType.ACCURACY, TokenType.SCALE, 
                    TokenType.INTELIGENCE, TokenType.WARFARE, 
                    TokenType.STEWARDSHIP, TokenType.CRIT_CHANCE
                );
        }
        private bool Match(params TokenType[] types)
        {
            foreach(TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private Token LookBack(int num)
        {
            return tokens[current - num];
        }
        private ParseError Error(Token tok, string message)
        {
            Console.WriteLine($"Parser Error at line {tok.line}: {message}");
            WrldBxScript.Error(tok.line, message);
            return new ParseError();
        }

    }
}
