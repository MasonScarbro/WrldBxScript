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


/* 
 * NOTES:
 * As far as effects and projectiles go,
 * I could do a couple of things I could process
 * it in post by rereading the traits File 
 * or I could just skip the Traits section 
 * of there code and process everything after
 * The goal obviously being that I can compile the traits
 * file with all the effects they added
 * regardless where their projectiles or effects
 * section is in their code
*/

namespace WrldBxScript
{
    class Compiler
    {
        private int count = 0;
        private string src;
        private string modname;
        private Dictionary<string, WrldBxEffect> effects = new Dictionary<string, WrldBxEffect>();
        private Dictionary<string, WrldBxTrait> traits = new Dictionary<string, WrldBxTrait>();
        private Dictionary<string, WrldBxEffect> projectiles = new Dictionary<string, WrldBxEffect>();
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
                    UpdateTraits(name.ToString(), stmtv.type, EvaluateExpr(stmtv.value));
                }

                if (type.Equals("EFFECTS"))
                {
                    UpdateEffects(name.ToString(), stmtv.type, EvaluateExpr(stmtv.value));
                }
                if (type.Equals("PROJECTILES"))
                {
                    switch (stmtv.type.type)
                    {
                        case TokenType.ID:
                            AddBlockId(name, type);
                            UpdateEffects(name.ToString(), stmtv.type, EvaluateExpr(stmtv.value));
                            break;
                        case TokenType.PATH:
                            src += "\t\t\ntexture = " + EvaluateExpr(stmtv.value) + ",";
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
                    
                    Execute(block, nameP, stmtst.type.lexeme);
                    //AddReqCodeToBlock(stmtst.type, nameP);
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
                effects[id].UpdateStats(type , value);
            }
            else
            {
                effects.Add(id, new WrldBxEffect(id));
                UpdateEffects(id, type, value);
            }
        }

        private void UpdateTraits(string id, Token type, object value)
        {
            if (traits.ContainsKey(id))
            {
                traits[id].UpdateStats(type, value);
            }
            else
            {
                traits.Add(id, new WrldBxTrait(id));
                UpdateTraits(id, type, value);
            }
        }

        private void UpdateProjectiles(string id, Token type, object value)
        {
            if (projectiles.ContainsKey(id))
            {
                projectiles[id].UpdateStats(type, value);
            }
            else
            {
                projectiles.Add(id, new WrldBxEffect(id));
                UpdateProjectiles(id, type, value);
            }
        }

        private bool TryGetCurrentEffect(string id, out WrldBxEffect effect)
        {
            if (effects.ContainsKey(id))
            {
                effect = effects[id];
                return true;
            }

            effect = null;
            return false;
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
                src += "World.world.stackEffects.CallMethod(" + "add" + $", {name});";
            }
            if (type.lexeme.Equals("PROJECTILES"))
            {
                src += "\t\t\n});";
                
            }
        }

        private void AddBlockId(object name, string type)
        {
            if (type.Equals("TRAITS"))
            {
                src += "\t\t\nActorTrait " + name.ToString() + " = new ActorTrait();";
                src += "\t\t\n" + name.ToString() + ".id = " + InQoutes(name.ToString()) + ';';
            }
            if (type.Equals("EFFECTS"))
            {
                src += "\t\tvar " + name.ToString() + " = AssetManager.effects_library.add(new EffectAsset {";
                src += "\t\t\nid = " + name.ToString();
            }
            if (type.Equals("PROJECTILES"))
            {
                src += "\t\tvar " + name.ToString() + "AssetManager.projectiles.add(new ProjectileAsset {";
                src += "\t\t\nid = " + name.ToString();
            }

        }

        private void GenerateCode(Token type)
        {
            if (type.lexeme.Equals("EFFECTS"))
            {
                foreach (WrldBxEffect effect in effects.Values)
                {
                    AddBlockId(effect.id, type.lexeme);
                    src += "\t\t\nsprite_path = " + InQoutes(effect.sprite_path) + ",";
                    src += "\t\t\ntime_between_frames = " + effect.time_between_frames + ",";
                    src += "\t\t\ndraw_light_area = " + effect.draw_light_area + ",";
                    src += "\t\t\ndraw_light_size = " + effect.draw_light_size + ",";
                    src += "\t\t\nlimit = " + effect.limit + ",";
                    AddReqCodeToBlock(type, effect.id);
                }
            }

            if (type.lexeme.Equals("TRAITS"))
            {
                foreach (WrldBxTrait trait in traits.Values)
                {
                    AddBlockId(trait.id, type.lexeme);
                    
                    src += ToStatString(trait.id, "health") + trait.health + ";";
                    src += ToStatString(trait.id, "damage") + trait.damage+ ";";
                    src += ToStatString(trait.id, "crit_chance") + trait.critChance + ";";
                    src += ToStatString(trait.id, "range") + trait.range + ";";
                    src += ToStatString(trait.id, "attack_speed") + trait.attackSpeed + ";";
                    src += ToStatString(trait.id, "dodge") + trait.dodge + ";"; 
                    src += ToStatString(trait.id, "accuracy") + trait.accuracy + ";";
                    src += ToStatString(trait.id, "scale") + (trait.scale / 100) + ";";
                    src += ToStatString(trait.id, "intelligence") + trait.intelligence + ";";
                    src += ToStatString(trait.id, "warfare") + trait.warfare + ";";
                    src += ToStatString(trait.id, "stewardship") + trait.stewardship + ";";
                    src += trait.id+ ".path_icon" + InQoutes(trait.pathIcon) + ";";

                    AddReqCodeToBlock(type, trait.id);
                }
            }
        }


        private void CompileToFile(Token type)
        {
            if (type.lexeme.Equals("TRAITS"))
            {
                GenerateCode(type);
                src += "\n\t}";
                src += BuildTraitPowerFunctions();
                src += Constants.TRAITSEOF;
                File.WriteAllText("C:/Users/Admin/Desktop/fart.cs", src);
                FormatCode("C:/Users/Admin/Desktop/fart.cs");


            }
            if (type.lexeme.Equals("EFFECTS"))
            {
                GenerateCode(type);
                src += "\n\t\t}\n\t}\n}";
                File.WriteAllText("C:/Users/Admin/Desktop/doodoo.cs", src);
                FormatCode("C:/Users/Admin/Desktop/doodoo.cs");
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

        private string InQoutes(string str)
        {
            return '"' + str + '"';
        }
        private string ToStatString(string nameP, string type)
        {
            return "\t\t\n" + ReplaceWhiteSpace(nameP.ToLower()) + ".base_stats[S." + type + "] += ";
        }

        private string BuildTraitPowerFunctions()
        {
            string powerFunc = "";
            foreach (WrldBxEffect effect in  effects.Values)
            {
                powerFunc = $"public static bool {effect.id}{(effect.IsAttack ? "Attack" : "Special")}(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)";
                powerFunc += "\n{";
                powerFunc += "if (pTarget != null)\n{";
                powerFunc += $"if (Toolbox.randomChance({effect.chance}))" + "\n{";
                powerFunc += $"EffectsLibrary.spawn({effect.id}, {(effect.spawnsOnTarget ? "pTarget.a.currentTile" : "pSelf.a.currentTile")}, null, null, 0f, -1f, -1f);";
                powerFunc += "\n}\n}\nreturn true;\n}\nreturn false;\n}";
            }

            return powerFunc;
        }

    }
}
