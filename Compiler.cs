﻿using System;
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

namespace WrldBxScript
{
    class Compiler
    {
        private int count = 0;
        private string src;
        private string modname;
        public void Compile(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    var stmt = Execute(statement, null);
                    Console.WriteLine(stmt);


                }
                //Console.WriteLine(src);
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
                if (stmtv.type.lexeme.ToUpper().Equals("MODNAME"))
                {
                    modname = VerifyModnameType(stmtv);
                }
                else if (name == null && modname == null)
                {
                    throw new CompilerError(stmtv.type, "Error You Should not have a variable outside of a block UNLESS its MODNAME");
                }
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
                src += "namespace " + modname + "\n{\n";
                src += "\t\nclass " + ToParaCase(stmtst.type.lexeme) + "\n" + "{" + "\n\tpublic static void init() \n{";
                foreach (Stmt.Block block in stmtst.body)
                {
                    string nameP = VerifyBlockName(block);
                    AddBlockId(nameP);
                    Execute(block, nameP);
                    AddReqCodeToBlock(stmtst.type, nameP);
                    count++;
                }
                CompileToFile(stmtst.type);
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
                        
                    default:
                        throw new CompilerError(exprB.oper, "Error With Processing Value After Identifier");
                        
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
        }

        private void AddBlockId(object name)
        {
            src += "\t\t\nActorTrait " + name.ToString() + " = new ActorTrait();";
            src += "\t\t\n" + name.ToString() + ".id = " + '"' + name.ToString() + '"' + ';';
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
        private async void FormatCode(string path)
        {
            
            string code = File.ReadAllText(path);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = await syntaxTree.GetRootAsync();
            var formattedRoot = Formatter.Format(root, Formatter.Annotation, new AdhocWorkspace());
            string formattedCode = formattedRoot.ToFullString();
            File.WriteAllText(path, formattedCode);
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
