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
using Microsoft.Build.Evaluation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static WrldBxScript.StringHelpers;
using WrldBxScript.Globals;



namespace WrldBxScript
{
    class Compiler
    {
        private int count = 0;
        private StringBuilder src = new StringBuilder();
        private string modname;

        private readonly CodeGeneratorFactory codeGeneratorFactory;
        private readonly Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> repositories;

        private readonly Dictionary<string, object> globals;

        public Compiler()
        {
            repositories = new Dictionary<string, WrldBxObjectRepository<IWrldBxObject>>
            {
                { "EFFECTS", new WrldBxObjectRepository<IWrldBxObject>(id => new WrldBxEffect(id)) },
                { "TRAITS", new WrldBxObjectRepository<IWrldBxObject>(id => new WrldBxTrait(id)) },
                { "PROJECTILES", new WrldBxObjectRepository<IWrldBxObject>(id => new WrldBxProjectile(id)) },
                { "STATUSES", new WrldBxObjectRepository<IWrldBxObject>(id => new WrldBxStatus(id)) },
                { "TERRAFORMING", new WrldBxObjectRepository<IWrldBxObject>(id => new WrldBxTerraform(id)) }
            };

            globals = new Dictionary<string, object>()
            {
                { "@slowness", "pTarget.addStatusEffect(\"slowness\");" },
                { "@poison", "pTarget.addStatusEffect(\"poisoned\");" },
                { "@teleportSelf", "ActionLibrary.teleportRandom(null, pSelf, null);" },
                { "@teleportTarget", "ActionLibrary.teleportRandom(null, pSelf, null);" },
                { "@wizardry", Constants.WIZRARDRY },
                { "@shakeWorld", "World.world.startShake(0.3f, 0.01f, 2f, true, true);" },
                { "@addTrait", new AddTrait(repositories) },
                { "@invincible", new Invincible() },
                {  "@shield", new Shield() },

            };

            codeGeneratorFactory = new CodeGeneratorFactory(repositories, globals);
        }


        public void Compile(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    var stmt = Execute(statement, null, "");
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

            switch (stmt)
            {
                case Stmt.Var stmtv:
                    HandleVarStmt(stmtv, name, type);
                    break;
                case Stmt.Starter stmtst:
                    HandleStarterStmt(stmtst);
                    break;
                case Stmt.Block stmtb:
                    HandleBlockStmt(stmtb, name, type);
                    break;
            }

            return null;
        }


        public void HandleVarStmt(Stmt.Var stmtv, object name, string type)
        {
            if (stmtv.type.lexeme.ToUpper().Equals("MODNAME"))
            {
                modname = VerifyModnameType(stmtv);

            }
            else if (name == null && modname == null)
            {
                throw new CompilerError(stmtv.type, "Error You Should not have a variable outside of a block UNLESS its MODNAME");
            }
            else
            {
                UpdateObjectByType(type, name.ToString(), stmtv.type, EvaluateExpr(stmtv.value));
            }
        }

        public void HandleStarterStmt(Stmt.Starter stmtst)
        {
            if (modname == null) modname = "MyDummyMod";
            src.AppendLine($"namespace {modname}");
            src.AppendLine("{");
            src.AppendLine($"\tclass {ToParaCase(stmtst.type.lexeme)}");
            src.AppendLine("\t{");

            foreach (Stmt.Block block in stmtst.body)
            {
                string nameP = VerifyBlockName(block);

                Execute(block, nameP, stmtst.type.lexeme);
                //AddReqCodeToBlock(stmtst.type, nameP);
                count++;
            }

            var generator = codeGeneratorFactory.GetGenerator(stmtst.type.lexeme);
            generator.GenerateCode(src, modname);
            CompileToFile(stmtst.type);
        }

        private void HandleBlockStmt(Stmt.Block stmtb, object name, string type)
        {
            foreach (var stat in stmtb.statements)
            {
                Execute(stat, name, type);
            }
        }

        private object EvaluateExpr(Expr expr)
        {
            if (expr is Expr.Grouping exprG)
            {
                if (exprG.expression is Expr.List exprList)
                {
                    return exprList.expressions.Select(EvaluateExpr).ToList();
                }
            }
            if (expr is Expr.Literal exprL)
            {
                if (exprL.value is bool) return exprL.value.ToString().ToLower();
                if (exprL.value is null) return "null";
                return exprL.value;

            }
            if (expr is Expr.Call call)
            {
                var name = EvaluateExpr(call.callee);
                var args = call.expressions.Select(EvaluateExpr).ToList();
                return (name, args);
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




        #region ObjectUpdateFuncs
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



        private void UpdateObjectByType(string type, string id, Token token, object value)
        {
            if (repositories.TryGetValue(type, out var repository))
            {
                repository.UpdateObject(id, token, value);
            }
            else
            {
                throw new CompilerError(new Token(token.type, type, null, 0),
                    $"Invalid object type: {type}");
            }
        }

        #endregion


        #region CodeGenAndCompilation




        private void CompileToFile(Token type)
        {
            if (type.lexeme.Equals("TRAITS"))
            {


                File.WriteAllText("C:/Users/Admin/Desktop/fart.cs", src.ToString());
                FormatCode("C:/Users/Admin/Desktop/fart.cs");


            }
            if (type.lexeme.Equals("EFFECTS"))
            {

                File.WriteAllText("C:/Users/Admin/Desktop/doodoo.cs", src.ToString());
                FormatCode("C:/Users/Admin/Desktop/doodoo.cs");
            }
            if (type.lexeme.Equals("STATUSES"))
            {

                File.WriteAllText("C:/Users/Admin/Desktop/statsus.cs", src.ToString());
                FormatCode("C:/Users/Admin/Desktop/statsus.cs");
            }
            if (type.lexeme.Equals("PROJECTILES"))
            {

                File.WriteAllText("C:/Users/Admin/Desktop/poopoo.cs", src.ToString());
                FormatCode("C:/Users/Admin/Desktop/poopoo.cs");
            }
            if (type.lexeme.Equals("TERRAFORMING"))
            {
                //src.Append("\n\t\t}\n\t}\n}");
                File.WriteAllText("C:/Users/Admin/Desktop/terra.cs", src.ToString());
                FormatCode("C:/Users/Admin/Desktop/terra.cs");
            }
            src.Clear(); //reset src for next starter
            count = 0;
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

        #endregion



        #region VerificationHelpers

        private string VerifyBlockName(Stmt.Block stmtb)
        {

            if (stmtb.statements[0] is Stmt.Var nameP)
            {
                if (src.ToString().Contains(" " + nameP.value + " "))
                {
                    throw new CompilerError(nameP.type, "Hmmm it looks Like you have already used " + nameP.value + " Somewhere in your code!");
                }
                if (nameP.type.type == TokenType.ID) return EvaluateExpr(nameP.value).ToString();
                //else
                throw new CompilerError(nameP.type, "Is Not a Name/Id, Each block MUST have an ID or NAME tag at the start of each block");
            }
            return null;
        }

        private string VerifyModnameType(Stmt.Var stmtv)
        {

            if (EvaluateExpr(stmtv.value) is string) return ToParaCase(ReplaceWhiteSpace(EvaluateExpr(stmtv.value).ToString()));
            //else
            throw new CompilerError(stmtv.type, "Modname CANNOT be an integer or double It MUST be a string!");
        }

        #endregion






    }
}
