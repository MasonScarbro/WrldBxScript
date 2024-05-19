using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    class Compiler
    {
        private int count = 0;
        private string src;
        public void Compile(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    var stmt = Execute(statement, null);
                    Console.WriteLine(stmt);


                }
                Console.WriteLine(src);
            }
            catch (CompilerError error)
            {
                WrldBxScript.CompilerErrorToCons(error);
            }
        }

        private Stmt Execute(Stmt stmt, object name)
        {
            
            if (stmt is Stmt.Var stmtv)
            {
                
                switch (stmtv.type.lexeme.ToUpper())
                {
                    case "HEALTH":
                        src += ToStatString(name.ToString(), "health") + EvaluateExpr(stmtv.value) + ";";
                        break;
                    case "DAMAGE":
                        src += ToStatString(name.ToString(), "damage") + EvaluateExpr(stmtv.value) + ";";
                        break;
                    case "CRIT_CHANCE":
                        src += ToStatString(name.ToString(), "crit_chance") + EvaluateExpr(stmtv.value) + ";";
                        break;
                    case "RANGE":
                        break;
                    case "ATTACK_SPEED":
                        src += ToStatString(name.ToString(), "attack_speed") + EvaluateExpr(stmtv.value) + ";";
                        break;
                    case "DODGE":
                        src += ToStatString(name.ToString(), "dodge") + EvaluateExpr(stmtv.value) + ";";
                        break;
                    case "ACCURACY":
                        src += ToStatString(name.ToString(), "accuracy") + EvaluateExpr(stmtv.value) + ";";
                        break;
                    case "SCALE":
                        src += ToStatString(name.ToString(), "scale") + EvaluateExpr(stmtv.value) + ";";
                        break;
                    case "INTELIGENCE":
                        src += ToStatString(name.ToString(), "intelligence") + EvaluateExpr(stmtv.value) + ";";
                        break;
                    case "WARFARE":
                        src += ToStatString(name.ToString(), "warfare") + EvaluateExpr(stmtv.value) + ";";
                        break;
                    case "STEWARDSHIP":
                        src += ToStatString(name.ToString(), "stewardship") + EvaluateExpr(stmtv.value) + ";";
                        break;


                }
                
            }
            if (stmt is Stmt.Starter stmtst)
            {
                src = "class " + ToParaCase(stmtst.type.lexeme) + "\n" + "{" + "\n\tpublic static void init() \n{";
                foreach (Stmt.Block block in stmtst.body)
                {
                    string nameP = VerifyBlockName(block);
                    AddBlockId(nameP);
                    Execute(block, nameP);
                    count++;
                }
                // reset src and count at the end of executing a starter since it descends
                //count = 0;
                //src = "";
            }
            if (stmt is Stmt.Block stmtb)
            {
                
                foreach (Stmt stat in stmtb.statements)
                {
                    Execute(stat, name);
                }
            }
            
            return null;
        }

        private object EvaluateExpr(Expr expr)
        {
            if (expr is Expr.Literal exprL)
            {
                if (exprL.value is bool) return exprL.value.ToString().ToLower();
                if (exprL.value is null) return "null";
                return exprL.value;

            }
            if (expr is Expr.Binary exprB)
            {
                object left = EvaluateExpr(exprB.left);
                object right = EvaluateExpr(exprB.right);
                switch (exprB.oper.type)
                {
                    case TokenType.PLUS:
                        if (left is double && right is double)
                        {
                            return (double)left + (double)right;
                        }

                        if (left.GetType() == typeof(String) && right.GetType() == typeof(String))
                        {
                            return (string)left + (string)right;
                        }
                        //String Concatenation!
                        if ((left is String && right is Double))
                        {
                            return (string)left + (string)right.ToString();
                        }
                        else if ((left is Double) && (right is String))
                        {
                            return (string)left.ToString() + (string)right;
                        }
                        
                        break;
                    case TokenType.MINUS:
                        return (double)left - (double)right;
                        break;
                    default:
                        throw new CompilerError(exprB.oper, "Error With Processing Value After Identifier");
                        return null;
                }
            }
            return expr;
        }
        
        private string ToParaCase(string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
        }

        private string VerifyBlockName(Stmt.Block stmtb)
        {
            
            if (stmtb.statements[0] is Stmt.Var nameP)
            {
                if (src.Contains(" " + nameP.value + " "))
                {
                    throw new CompilerError(nameP.type, "Hmmm it looks Like you have already used " + nameP.value + " Somewhere in your code!");
                }
                if (nameP.type.type == TokenType.ID) return EvaluateExpr(nameP.value).ToString();
                //else
                throw new CompilerError(nameP.type, "Is Not a Name/Id, Each trait MUST have an ID or NAME tag at the start of each block");
            }
            return null;
        }

        private void AddBlockId(object name)
        {
            src += "\t\t\nActorTrait " + name.ToString() + " = new ActorTrait();";
            src += "\t\t\n" + name.ToString() + ".id = " + '"' + name.ToString() + '"' + ';';
        }

        private string ToStatString(string nameP, string type)
        {
            return "\t\t\n" + nameP.ToLower() + ".base_stats[S." + type + "] += ";
        }
    }
}
