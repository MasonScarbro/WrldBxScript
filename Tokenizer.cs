using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    class Tokenizer
    {

        private readonly String source;
        private readonly List<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 1;

        //Init
        public Tokenizer(string source)
        {
            this.source = source;
        }

        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
        {

            { "MODNAME", TokenType.MODNAME },
            { "TRAITS", TokenType.TRAITS },
            { "EFFECTS", TokenType.EFFECTS },
            { "STATUSES", TokenType.STATUSES },
            { "PROJECTILES", TokenType.PROJECTILES}, 
            { "DAMAGE", TokenType.DAMAGE },
            { "HEALTH", TokenType.HEALTH },
            { "ATTACK_SPEED", TokenType.ATTACK_SPEED },
            { "RANGE", TokenType.RANGE },
            { "LOCALIZTION", TokenType.LOCALIZATION },
            { "CRIT_CHANCE", TokenType.CRIT_CHANCE },
            { "ID", TokenType.ID },
            { "POWERS", TokenType.POWER },
            { "EFFECTNAMES", TokenType.POWER },
            { "NAME", TokenType.ID },
            { "EMPTY", TokenType.NIL },
            { "TRUE", TokenType.TRUE },
            { "FALSE", TokenType.FALSE },
            { "PATH", TokenType.PATH },
            { "ICON", TokenType.PATH },
            { "SPRITE", TokenType.PATH },
            { "TEXTURE", TokenType.PATH },
            { "LIMIT", TokenType.LIMIT },
            { "DRAW_LIGHT", TokenType.DRAW_LIGHT },
            { "LIGHT_SIZE", TokenType.DRAW_LIGHT_SIZE },
            { "FRAME_INTERVAL", TokenType.TIMEBETWEENFRAMES },
            { "TIMEBETWEENFRAMES", TokenType.TIMEBETWEENFRAMES },
            { "SPAWNS_FROM_ACTOR", TokenType.SPAWNFROMACTOR },
            { "TRAVELS_TO_TARGET", TokenType.SPAWNFROMACTOR },
            { "SPAWNS_ON_TARGET", TokenType.SPAWNONTARGET },
            { "SPAWNSONTARGET", TokenType.SPAWNFROMACTOR },
            { "IS_ATTACK", TokenType.ISATTK },
            { "ATTACK_EFFECT", TokenType.ISATTK },
            { "CHANCE", TokenType.CHANCE },
            { "SPEED", TokenType.SPEED },
            { "PROJECTILE_SPEED", TokenType.SPEED },
            { "PARABOLIC", TokenType.PARABOLIC },
            { "ARCHES", TokenType.PARABOLIC },
            { "LOOKING_AT_TARGET", TokenType.FACINGTRGT},
            { "ALWAYS_FACING_TARGET", TokenType.FACINGTRGT},







        };

        public List<Token> scanTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }
            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Consume();
            Console.WriteLine($"Scanning character: {c}, Line: {line}, Current Index: {current}");
            Console.WriteLine($"Start: {start}, Current: {current}, Current Text: {source.Substring(start, current - start)}");
            switch (c)
            {

                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case '[': AddToken(TokenType.LEFT_BRACKET); break;
                case ']': AddToken(TokenType.RIGHT_BRACKET); break;
                case ':': AddToken(TokenType.COLON); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    line++;
                    break;
                default:
                    if (IsDigit(c))
                    {
                        IsNumber();
                    } else if (IsAlpha(c))
                    {
                        IsKeyword();
                    }
                    else
                    {
                        WrldBxScript.Error(line, "Unexpected Char");
                    }

                    break;

            }
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z');
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsAlphaNum(char c)
        {
            return IsAlpha(c) || IsDigit(c) || IsSpaceSubstitute(c);
        }

        private bool IsSpaceSubstitute(char c)
        {
            return c == '_';
        }

        // check the char without consuming
        public char Peek()
        {
            if (IsAtEnd())
            {
                return '\0';
            }
            return source[current];
        }
        public char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        //Is it at the end of file?
        public bool IsAtEnd()
        {
            return current >= source.Length;
        }

        //Move to the next char (consume the current and move one) 
        // returns current
        // increments which "consumes"
        public char Consume()
        {
            return source[current++];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void IsKeyword()
        {
            while (IsAlphaNum(Peek())) Consume();
            String text = source.Substring(start, current - start);
            if (keywords.TryGetValue(text.ToUpper(), out TokenType type) == false)
            {
                type = TokenType.STRING;
            }
            AddToken(type);
        }

        private void IsNumber()
        {
            while (IsDigit(Peek())) Consume();

            if (Peek() == '.' && IsDigit(Peek()))
            {
                Consume();
                while (IsDigit(Peek())) Consume();
            }

            var result = source.Substring(start, current - start);
            Console.WriteLine("Number:  " + result);
            AddToken(TokenType.NUMBER, Double.Parse(result));
        }

        // The Real Method, If only the type is given then literal is null Which is what AddToken(type) does.
        private void AddToken(TokenType type, Object literal)
        {
            String text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        } 
    }
}
