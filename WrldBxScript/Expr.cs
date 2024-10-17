using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public abstract class Expr
    {
		public class Binary : Expr
		{
			public Binary(Expr left, Token oper, Expr right)
			{
				this.left = left;
				this.oper = oper;
				this.right = right;
			}

			public readonly Expr left;
			public readonly Token oper;
			public readonly Expr right;
		}
		public class Grouping : Expr
		{
			public Grouping(Expr expression)
			{
				this.expression = expression;
			}


			public readonly Expr expression;
		}

        public class List : Expr
        {
            public List(List<Expr> expressions)
            {
                this.expressions = expressions;
            }


            public readonly List<Expr> expressions;
		}
		public class Literal : Expr
		{
			public Literal(object value)
			{
				this.value = value;
			}


			public readonly object value;
		}
		public class Logical : Expr
		{
			public Logical(Expr left, Token oper, Expr right)
			{
				this.left = left;
				this.oper = oper;
				this.right = right;
			}


			public readonly Expr left;
			public readonly Token oper;
			public readonly Expr right;
		}
		public class Unary : Expr
		{
			public Unary(Token oper, Expr right)
			{
				this.oper = oper;
				this.right = right;
			}

		

			public readonly Token oper;
			public readonly Expr right;
		}
	}
}
