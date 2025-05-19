using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public abstract class Stmt
    {

        /*
         * The Idea is Each Keyword will be a "var" and "initialize" what ever the expression is in the Code gen
         * Each Var Will be in a traits block '[' and ']' NOT '{' and '}' these are \n chars
         * in Code gen
         */
        public class Var : Stmt
        {
            public Var(Token type, Expr value)
            {
                this.type = type;
                this.value = value;
            }
            public readonly Token type; // This is the Identifier in a way 
            public readonly Expr value;
        }

        // These will be the starters like TRAITS, EFFECTS, Etc...
        /*
         * I havent figured out how to do this just yet but the plan is to check the type
         * and treat anything after this as a sort of block where all the 
         * Stmts are made sure to only be in that file, It is imperative I know 
         * when the end is and what type for proper localization
         * 
         */
        public class Starter : Stmt
        {
            public Starter(Token type, List<Stmt> body)
            {
                this.body = body;
                this.type = type;
            }

            public readonly Token type;
            public readonly List<Stmt> body;
            
        }

        // In code gen I will do a count to handle the variable names ex. "var" + count for each stmt
        public class Block : Stmt
        {
            public Block(List<Stmt> statements)
            {
                this.statements = statements;

            }

            public readonly List<Stmt> statements;

        }

        public class Expression : Stmt
        {
            public Expression(Expr expression)
            {
                this.expression = expression;
            }

            public readonly Expr expression;
        }

    }
}
