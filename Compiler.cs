using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Options;
using System.Threading;

namespace WrldBxScript
{
    class Compiler
    {
        private int count = 0;
        private string src;
        private string modname;
        private Dictionary<string, WrldBxEffect> effects = new Dictionary<string, WrldBxEffect>();
        public void Compile(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    var stmt = Execute(statement, null, null);
                    Console.WriteLine(stmt);


                }
                //Console.WriteLine(src);
            }
            catch (CompilerError error)
            {
                WrldBxScript.CompilerErrorToCons(error);
            }
        }

        private Stmt Execute(Stmt stmt, object name, string type)
        {
            
            if (stmt is Stmt.Var stmtv)
            {
                if (stmtv.type.lexeme.ToUpper().Equals("MODNAME"))
                {
                    modname = VerifyModnameType(stmtv);
                }
                else if (name == null && modname == null)
                {
                    throw new CompilerError(stmtv.type, "Error You Should not have a variable outside of a block UNLESS its MODNAME");
                }
                if (type.Equals("TRAITS"))
                {
                    switch (stmtv.type.type)
                    {

                        case TokenType.HEALTH:
                            src += ToStatString(name.ToString(), "health") + EvaluateExpr(stmtv.value) + ";";
                            break;
                        case TokenType.DAMAGE:
                            src += ToStatString(name.ToString(), "damage") + EvaluateExpr(stmtv.value) + ";";
                            break;
                        case TokenType.CRIT_CHANCE:
                            src += ToStatString(name.ToString(), "crit_chance") + EvaluateExpr(stmtv.value) + ";";
                            break;
                        case TokenType.RANGE:
                            src += ToStatString(name.ToString(), "range") + EvaluateExpr(stmtv.value) + ";";
                            break;
                        case TokenType.ATTACK_SPEED:
                            src += ToStatString(name.ToString(), "attack_speed") + EvaluateExpr(stmtv.value) + ";";
                            break;
                        case TokenType.DODGE:
                            src += ToStatString(name.ToString(), "dodge") + EvaluateExpr(stmtv.value) + ";";
                            break;
                        case TokenType.ACCURACY:
                            src += ToStatString(name.ToString(), "accuracy") + EvaluateExpr(stmtv.value) + ";";
                            break;
                        case TokenType.SCALE:
                            src += ToStatString(name.ToString(), "scale") + EvaluateExpr(stmtv.value) + ";";
                            break;
                        case TokenType.INTELIGENCE:
                            src += ToStatString(name.ToString(), "intelligence") + EvaluateExpr(stmtv.value) + ";";
                            break;
                        case TokenType.WARFARE:
                            src += ToStatString(name.ToString(), "warfare") + EvaluateExpr(stmtv.value) + ";";
                            break;
                        case TokenType.STEWARDSHIP:
                            src += ToStatString(name.ToString(), "stewardship") + EvaluateExpr(stmtv.value) + ";";
                            break;
                        case TokenType.PATH:
                            src += name.ToString() + ".path_icon" + EvaluateExpr(stmtv.value) + ";";
                            break;
                        default:
                            throw new CompilerError(stmtv.type, "This keyword does not exist within the " + type + " block");
                    }
                }

                if (type.Equals("EFFECTS"))
                {
                    switch (stmtv.type.type)
                    {

                        case TokenType.PATH:
                            src += "\t\t\nsprite_path = " + EvaluateExpr(stmtv.value) + ",";
                            break;
                        case TokenType.TIMEBETWEENFRAMES:
                            src += "\t\t\ntime_between_frames = " + EvaluateExpr(stmtv.value) + ",";
                            break;
                        case TokenType.DRAW_LIGHT:
                            src += "\t\t\ndraw_light_area = " + EvaluateExpr(stmtv.value) + ",";
                            break;
                        case TokenType.DRAW_LIGHT_SIZE:
                            src += "\t\t\ndraw_light_size = " + EvaluateExpr(stmtv.value) + ",";
                            break;
                        case TokenType.LIMIT:
                            src += "\t\t\nlimit = " + EvaluateExpr(stmtv.value) + ",";
                            break;
                        default:
                            throw new CompilerError(stmtv.type, "This keyword does not exist within the " + type + " block");


                    }
                }
                
                
            }
            if (stmt is Stmt.Starter stmtst)
            {
                if (modname == null) modname = "MyDummyMod";
                src += "namespace " + modname + "\n{\n";
                src += "\t\nclass " + ToParaCase(stmtst.type.lexeme) + "\n" + "{" + "\n\tpublic static void init() \n{";

                foreach (Stmt.Block block in stmtst.body)
                {
                    string nameP = VerifyBlockName(block);
                    AddBlockId(nameP, stmtst.type.lexeme);
                    Execute(block, nameP, stmtst.type.lexeme);
                    AddReqCodeToBlock(stmtst.type, nameP);
                    count++;
                }
                CompileToFile(stmtst.type);
            }
            if (stmt is Stmt.Block stmtb)
            {
                
                foreach (Stmt stat in stmtb.statements)
                {
                    Execute(stat, name, type);
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
                        
                    default:
                        throw new CompilerError(exprB.oper, "Error With Processing Value After Identifier");
                        
                }
            }
            return expr;
        }


        /// <summary>
        /// Each time we encounter a value that changes its current effect we
        /// Check to see if that effect already exists if it does then we 
        /// simply update the corresponding value if it doesnt exist we create a new one
        /// once one is created and added we go back and make sure the value was 
        /// properly updated since the values arent part of the constructer
        /// side note: The reason we left the values out of the constructer 
        /// is for slightly better scalability, when we add a new value to be
        /// handled we would need to update the creation of a new object here
        /// that would be annoying instead a new value only requires editing in the 
        /// tokenizer, parser, and a slight addition to WrldBxEffect
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        private void UpdateEffects(string id, Token type, object value)
        {
            if (effects.ContainsKey(id))
            {
                effects[id].UpdateStats(new List<string> { type.lexeme }, value);
            }
            else
            {
                effects.Add(id, new WrldBxEffect(id));
                UpdateEffects(id, type, value);
            }
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
                throw new CompilerError(nameP.type, "Is Not a Name/Id, Each block MUST have an ID or NAME tag at the start of each block");
            }
            return null;
        }

        private void AddReqCodeToBlock(Token type, object name)
        {
            if (type.lexeme.Equals("TRAITS"))
            {
                src += "\t\t\nAssetManger.traits.add(" + name.ToString() + ");";
                src += "\t\t\nPlayerConfig.unlockTrait(" + name.ToString() + ".id);\n";
            }
            if (type.lexeme.Equals("EFFECTS"))
            {
                src += "\t\t\n});";
            }
        }

        private void AddBlockId(object name, string type)
        {
            if (type.Equals("TRAITS"))
            {
                src += "\t\t\nActorTrait " + name.ToString() + " = new ActorTrait();";
                src += "\t\t\n" + name.ToString() + ".id = " + '"' + name.ToString() + '"' + ';';
            }
            if (type.Equals("EFFECTS"))
            {
                src += "\t\tvar " + name.ToString() + "AssetManager.effects_library.add(new EffectAsset {";
                src += "\t\t\nid = " + name.ToString();
            }
            
        }


        private void CompileToFile(Token type)
        {
            if (type.lexeme.Equals("TRAITS"))
            {
                src += Constants.TRAITSEOF;
                File.WriteAllText("C:/Users/Admin/Desktop/fart.cs", src);
                FormatCode("C:/Users/Admin/Desktop/fart.cs");


            }
            src = ""; //reset src for next starter
            count = 0;
        }

        private string VerifyModnameType(Stmt.Var stmtv)
        {
            if (EvaluateExpr(stmtv.value) is string) return ToParaCase(ReplaceWhiteSpace(EvaluateExpr(stmtv.value).ToString()));
            //else
            throw new CompilerError(stmtv.type, "Modname CANNOT be an integer or double It MUST be a string!");
        }
        private void FormatCode(string path, CancellationToken cancelToken = default)
        {
            
            string code = File.ReadAllText(path);
            string source = CSharpSyntaxTree.ParseText(code)
                            .GetRoot(cancelToken)
                            .NormalizeWhitespace()
                            .SyntaxTree
                            .GetText(cancelToken)
                            .ToString();
            File.WriteAllText(path, source);
        }

        private string ReplaceWhiteSpace(string str)
        {
            try
            {
                str.Replace(' ', '_');
            }
            catch (Exception)
            {

            }
            
            return str;
        }

        private string ToStatString(string nameP, string type)
        {
            return "\t\t\n" + ReplaceWhiteSpace(nameP.ToLower()) + ".base_stats[S." + type + "] += ";
        }
    }
}
