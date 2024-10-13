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
        private Dictionary<string, WrldBxProjectile> projectiles = new Dictionary<string, WrldBxProjectile>();
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
                    UpdateProjectiles(name.ToString(), stmtv.type, EvaluateExpr(stmtv.value));
                }


            }
            if (stmt is Stmt.Starter stmtst)
            {
                if (modname == null) modname = "MyDummyMod";
                src += "namespace " + modname + "\n{\n";
                src += "\t\nclass " + ToParaCase(stmtst.type.lexeme) + "\n" + "{";

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
        private void UpdateEffects(string id, Token type, object value)
        {
            if (effects.ContainsKey(id))
            {
                effects[id].UpdateStats(type, value);
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
                projectiles.Add(id, new WrldBxProjectile(id));
                UpdateProjectiles(id, type, value);
            }
        }

        #endregion


        #region CodeGenAndCompilation

        private void GenerateCode(Token type)
        {

            src += "\n\tpublic static void init() \n{";
            var sb = new StringBuilder();
            if (type.lexeme.Equals("EFFECTS"))
            {
                foreach (WrldBxEffect effect in effects.Values)
                {
                    AddBlockId(sb, effect.id, type.lexeme);
                    sb.Append($"\t\t\nsprite_path = {InQoutes(effect.sprite_path)}," +
                           $"\t\t\ntime_between_frames = {effect.time_between_frames}," +
                           $"\t\t\ndraw_light_area = {effect.draw_light_area}," +
                           $"\t\t\ndraw_light_size = {effect.draw_light_size}," +
                           $"\t\t\nlimit = {effect.limit},");
                    AddReqCodeToBlock(sb, type, effect.id);
                }

                sb.Append("\n\t\t}\n\t}\n}");
                src += sb.ToString();
            }
            if (type.lexeme.Equals("TRAITS"))
            {

                var funcs = new StringBuilder();
                foreach (WrldBxTrait trait in traits.Values)
                {


                    AddBlockId(sb, trait.id, type.lexeme);
                    AddIfHasValue(sb, trait.health, "health", trait.id);
                    AddIfHasValue(sb, trait.damage, "damage", trait.id);
                    AddIfHasValue(sb, trait.critChance, "crit_chance", trait.id);
                    AddIfHasValue(sb, trait.range, "range", trait.id);
                    AddIfHasValue(sb, trait.attackSpeed, "attack_speed", trait.id);
                    AddIfHasValue(sb, trait.dodge, "dodge", trait.id);
                    AddIfHasValue(sb, trait.accuracy, "accuracy", trait.id);
                    AddIfHasValue(sb, trait.scale, "scale", trait.id, true);
                    AddIfHasValue(sb, trait.intelligence, "intelligence", trait.id);
                    AddIfHasValue(sb, trait.warfare, "warfare", trait.id);
                    AddIfHasValue(sb, trait.stewardship, "stewardship", trait.id);
                    sb.AppendLine($"{trait.id}.path_icon = {InQoutes(trait.pathIcon)};");
                    AddReqCodeToBlock(sb, type, trait.id);

                    funcs.Append(BuildTraitPowerFunctions(trait));

                }

                sb.Append("\n\t}");
                sb.Append(funcs);
                sb.Append(Constants.TRAITSEOF);
                src += sb.ToString();
            }
            if (type.lexeme.Equals("PROJECTILES"))
            {
                foreach (WrldBxProjectile projectile in projectiles.Values)
                {
                    AddBlockId(sb, projectile.id, type.lexeme);
                    if (projectile.texture.Equals("fireball"))
                    {
                        WrldBxScript.Warning($"{projectile.id} Does not have an assigned texture, given default texture");
                    }
                    sb.Append($"\t\t\ndraw_light_area = {projectile.draw_light_area}," +
                           $"\t\t\ndraw_light_size = {projectile.draw_light_size}," +
                           $"\t\t\ntexture = {InQoutes(projectile.texture)},");
                    sb.Append(projectile.animation_speed.HasValue ? $"\t\t\nanimation_speed = {projectile.animation_speed}" : "");
                    sb.Append($"\t\t\nspeed = {projectile.speed}," +
                           $"\t\t\nparabolic = {projectile.parabolic}" +
                           $"\t\t\nlook_at_target = {projectile.lookAtTarget}" +
                           $"\t\t\nstartScale = {projectile.scale}," +
                           $"\t\t\ntargetScale = {projectile.scale},");
                    sb.Append("\t\t\nlooped = true," +
                           "\t\t\nendEffect = string.Empty," +
                           $"\t\t\ntexture_shadow = {InQoutes("shadow_ball")}," +
                           $"\t\t\ntrailEffect_enabled = true," +
                           $"sound_launch = {InQoutes("event:/SFX/WEAPONS/WeaponFireballStart")},");


                    AddReqCodeToBlock(sb, type, projectile.id);
                }
                src += sb.ToString();
            }

            sb.Clear();
        }



        private void CompileToFile(Token type)
        {
            if (type.lexeme.Equals("TRAITS"))
            {
                GenerateCode(type);
                File.WriteAllText("C:/Users/Admin/Desktop/fart.cs", src);
                FormatCode("C:/Users/Admin/Desktop/fart.cs");


            }
            if (type.lexeme.Equals("EFFECTS"))
            {
                GenerateCode(type);
                File.WriteAllText("C:/Users/Admin/Desktop/doodoo.cs", src);
                FormatCode("C:/Users/Admin/Desktop/doodoo.cs");
            }
            src = ""; //reset src for next starter
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

        private string VerifyModnameType(Stmt.Var stmtv)
        {
            if (EvaluateExpr(stmtv.value) is string) return ToParaCase(ReplaceWhiteSpace(EvaluateExpr(stmtv.value).ToString()));
            //else
            throw new CompilerError(stmtv.type, "Modname CANNOT be an integer or double It MUST be a string!");
        }

        #endregion


        #region StringHelpers

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
        private string ToParaCase(string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
        }

        #endregion


        #region CodeGenHelpers
        // ####################### TRAIT HELPERS ################################ //
        private string BuildTraitPowerFunctions(WrldBxTrait trait)
        {

            string powerFuncs = "";
            string mainAttackFunc =
                $"public static bool {trait.id}Attack(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)" +
                "\n{" +
                "\n\tif (pTarget != null)" +
                "\n\t{";

            string mainSpecialFunc =
                $"public static bool {trait.id}Special(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)" +
                "\n{" +
                "\n\tif (pSelf.a != null)" +
                "\n\t{";


            foreach (string effectKey in trait.effectName)
            {

                if (effects.TryGetValue(effectKey, out WrldBxEffect effect))
                {

                    //TODO: build the effect
                    powerFuncs += $"public static bool {effect.id}(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)" +
                                  "\n{" +
                                  "\n\tif (pTarget != null)" +
                                  "\n\t{" +
                                  $"\n\t\t\tEffectsLibrary.spawn({InQoutes(effect.id)}, {(effect.spawnsOnTarget ? "pTarget.a.currentTile" : "pSelf.a.currentTile")}, null, null, 0f, -1f, -1f);" +
                                  "\n\t\treturn true;" +
                                  "\n\t}" +
                                  "\n\t\treturn false;" +
                                  "\n}";
                    if (effect.IsAttack)
                    {
                        mainAttackFunc +=
                            $"\n\t\tif (Toolbox.randomChance({effect.chance}))" +
                            "\n\t\t{" +
                            $"\n\t\t\t{effect.id}(pSelf, pTarget, pTile);" +
                            "\n\t\t}";

                    }
                    else
                    {
                        mainSpecialFunc +=
                            $"\n\t\tif (Toolbox.randomChance({effect.chance}))" +
                            "\n\t\t{" +
                            $"\n\t\t\t{effect.id}(pSelf, pTarget, pTile);" +
                            "\n\t\t}";
                    }
                }
                else
                {
                    WrldBxScript.Warning($"Could not find {effectKey}" +
                                         $" in your effects. FAILED " +
                                         $"to build power for it");

                }

            }

            mainSpecialFunc += "\n\t\treturn true;" +
                               "\n\t}" +
                               "\n\treturn false;" +
                               "\n}";
            mainAttackFunc += "\n\t\treturn true;" +
                              "\n\t}" +
                              "\n\treturn false;" +
                              "\n}";
            return $"{mainSpecialFunc}\n{mainAttackFunc}\n{powerFuncs}";

        }

        private string BuildDefaultPowerFunctions()
        {
            string powerFunc = "";
            foreach (WrldBxEffect effect in effects.Values)
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

        private void AddIfHasValue<T>(StringBuilder sb, T? value, string statName, string traitId, bool divideBy100 = false) where T : struct
        {
            if (value.HasValue)
            {
                string statValue = divideBy100 ? (Convert.ToDouble(value.Value) / 100).ToString() : value.ToString();
                sb.AppendLine($"{ToStatString(traitId, statName)}{statValue};");
            }
        }

        // ################################################################## //

        // ####################### GENERAL HELPERS ################################ //
        private void AddReqCodeToBlock(StringBuilder sb, Token type, object name)
        {
            if (type.lexeme.Equals("TRAITS"))
            {
                sb.Append("\t\t\nAssetManger.traits.add(" + name.ToString() + ");");
                sb.Append("\t\t\nPlayerConfig.unlockTrait(" + name.ToString() + ".id);\n");
            }
            if (type.lexeme.Equals("EFFECTS"))
            {
                sb.Append("\t\t\n});");
                sb.Append("World.world.stackEffects.CallMethod(" + "add" + $", {InQoutes(name.ToString())});");
            }
            if (type.lexeme.Equals("PROJECTILES"))
            {
                sb.Append("\t\t\n});");

            }
        }

        private void AddBlockId(StringBuilder sb, object name, string type)
        {
            if (type.Equals("TRAITS"))
            {
                sb.Append($"\t\t\nActorTrait {name} = new ActorTrait();");
                sb.Append($"\t\t\n{name}.id = {InQoutes(name.ToString())};");
            }
            if (type.Equals("EFFECTS"))
            {
                sb.Append("\t\tvar " + name.ToString() + " = AssetManager.effects_library.add(new EffectAsset {");
                sb.Append("\t\t\nid = " + InQoutes(name.ToString()));
            }
            if (type.Equals("PROJECTILES"))
            {
                sb.Append("\t\tvar " + name.ToString() + "AssetManager.projectiles.add(new ProjectileAsset {");
                sb.Append("\t\t\nid = " + InQoutes(name.ToString()));
            }

        }
        // ################################################################## //

        #endregion

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
    }
}
