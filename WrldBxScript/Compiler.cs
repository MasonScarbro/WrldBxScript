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




namespace WrldBxScript
{
    class Compiler
    {
        private int count = 0;
        private StringBuilder src = new StringBuilder();
        private string modname;
        private Dictionary<string, WrldBxEffect> effects = new Dictionary<string, WrldBxEffect>();
        private Dictionary<string, WrldBxTrait> traits = new Dictionary<string, WrldBxTrait>();
        private Dictionary<string, WrldBxProjectile> projectiles = new Dictionary<string, WrldBxProjectile>();
        private Dictionary<string, WrldBxTerraform> terraformOptions = new Dictionary<string, WrldBxTerraform>();
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
                else
                {
                    UpdateObjectByType(type, name.ToString(), stmtv.type, EvaluateExpr(stmtv.value));

                    
                }

                //If the type has an object to update, do it
                
                

            }
            if (stmt is Stmt.Starter stmtst)
            {
                if (modname == null) modname = "MyDummyMod";
                src.Append("namespace " + modname + "\n{\n");
                src.Append("\t\nclass " + ToParaCase(stmtst.type.lexeme) + "\n" + "{");

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
        

        private void UpdateObjects<T>(Dictionary<string, T> dictionary, string id, Token type, object value, Func<string, T> createObject) where T : IWrldBxObject
        {
            if (dictionary.ContainsKey(id))
            {
                dictionary[id].UpdateStats(type, value);
            }
            else
            {
                dictionary.Add(id, createObject(id));
                dictionary[id].UpdateStats(type, value);
            }
        }
        private void UpdateObjectByType(string objectType, string id, Token type, object value)
        {
            switch (objectType)
            {
                case "EFFECTS":
                    UpdateObjects(effects, id, type, value, newId => new WrldBxEffect(newId));  
                    break;
                case "TRAITS":
                    UpdateObjects(traits, id, type, value, newId => new WrldBxTrait(newId));    
                    break;
                case "PROJECTILES":
                    UpdateObjects(projectiles, id, type, value, newId => new WrldBxProjectile(newId));  
                    break;
                case "TERRAFORMING":
                    UpdateObjects(terraformOptions, id, type, value, newId => new WrldBxTerraform(newId));  
                    break;
                //NEW_MAJOR_UPDATE_HERE  

            }
        }

        #endregion


        #region CodeGenAndCompilation

        private void GenerateCode(Token type)
        {

            src.Append("\n\tpublic static void init() \n{");
            switch (type.lexeme)
            {
                case "EFFECTS":
                    foreach (WrldBxEffect effect in effects.Values)
                    {
                        AddBlockId(src, effect.id, type.lexeme);
                        src.Append($"\t\t\nsprite_path = {InQoutes(effect.sprite_path)}," +
                               $"\t\t\ntime_between_frames = {effect.time_between_frames}," +
                               $"\t\t\ndraw_light_area = {effect.draw_light_area}," +
                               $"\t\t\ndraw_light_size = {effect.draw_light_size}," +
                               $"\t\t\nlimit = {effect.limit},");
                        AddReqCodeToBlock(src, type, effect.id);
                    }

                    src.Append("\n\t\t}\n\t}\n}");
                    break;
                case "TRAITS":
                    var funcs = new StringBuilder();
                    foreach (WrldBxTrait trait in traits.Values)
                    {


                        AddBlockId(src, trait.id, type.lexeme);
                        AddIfHasValue(src, trait.health, "health", trait.id);
                        AddIfHasValue(src, trait.damage, "damage", trait.id);
                        AddIfHasValue(src, trait.critChance, "crit_chance", trait.id);
                        AddIfHasValue(src, trait.range, "range", trait.id);
                        AddIfHasValue(src, trait.attackSpeed, "attack_speed", trait.id);
                        AddIfHasValue(src, trait.dodge, "dodge", trait.id);
                        AddIfHasValue(src, trait.accuracy, "accuracy", trait.id);
                        AddIfHasValue(src, trait.scale, "scale", trait.id, true);
                        AddIfHasValue(src, trait.intelligence, "intelligence", trait.id);
                        AddIfHasValue(src, trait.warfare, "warfare", trait.id);
                        AddIfHasValue(src, trait.stewardship, "stewardship", trait.id);
                        src.AppendLine($"{trait.id}.path_icon = {InQoutes(trait.pathIcon)};");
                        src.AppendLine($"{trait.id}.action_attack_target = new AttackAction({trait.id});");
                        src.AppendLine($"{trait.id}.action_special_effect = (WorldAction)Delegate.Combine({trait.id}.action_special_effect, new WorldAction({trait.id}Attack));");
                        AddReqCodeToBlock(src, type, trait.id);

                        funcs.Append(BuildTraitPowerFunctions(trait));

                    }

                    src.Append("\n\t}");
                    src.Append(funcs);
                    src.Append(Constants.TRAITSEOF);
                    break;
                case "PROJECTILES":
                    foreach (WrldBxProjectile projectile in projectiles.Values)
                    {
                        AddBlockId(src, projectile.id, type.lexeme);
                        if (projectile.texture.Equals("fireball"))
                        {
                            WrldBxScript.Warning($"{projectile.id} Does not have an assigned texture, given default texture");
                        }
                        if (terraformOptions.ContainsKey(projectile.terraformOption))
                        {
                            //When we introduce Globals we may need to change this
                            throw new CompilerError(type, $"{projectile.terraformOption} Could not be found in your TERRAFORMING block");
                        }
                        src.Append($"\t\t\ndraw_light_area = {projectile.draw_light_area}," +
                               $"\t\t\ndraw_light_size = {projectile.draw_light_size}," +
                               $"\t\t\ntexture = {InQoutes(projectile.texture)},");
                        src.Append(projectile.animation_speed.HasValue ? $"\t\t\nanimation_speed = {projectile.animation_speed}" : "");
                        src.Append($"\t\t\nspeed = {projectile.speed}," +
                               $"\t\t\nparabolic = {projectile.parabolic}" +
                               $"\t\t\nlook_at_target = {projectile.lookAtTarget}" +
                               $"\t\t\nstartScale = {projectile.scale}," +
                               $"\t\t\ntargetScale = {projectile.scale}," +
                               $"\t\t\nterraformOption = {projectile.terraformOption},");
                        src.Append("\t\t\nlooped = true," +
                               "\t\t\nendEffect = string.Empty," +
                               $"\t\t\ntexture_shadow = {InQoutes("shadow_ball")}," +
                               $"\t\t\ntrailEffect_enabled = true," +
                               $"sound_launch = {InQoutes("event:/SFX/WEAPONS/WeaponFireballStart")},");


                        AddReqCodeToBlock(src, type, projectile.id);
                    }
                    src.Append("\n\t\t}\n\t}\n}");
                    break;
                case "TERRAFORMING":
                    foreach (WrldBxTerraform terraformOption in terraformOptions.Values)
                    {
                        AddBlockId(src, terraformOption.id, type.lexeme);
                        if (terraformOption.explode_strength.HasValue && terraformOption.explode_tile == false)
                        {
                            terraformOption.explode_tile = true;
                        }
                        if (!terraformOption.explode_strength.HasValue && terraformOption.explode_tile == true)
                        {
                            WrldBxScript.Warning($"You did not set explode_strength it was given a default value of 1");
                        }
                        src.Append(
                            $"flash = {terraformOption.flash}," +
                            $"explode_tile = {terraformOption.explode_tile}," +
                            $"applyForce = {terraformOption.applyForce}," +
                            $"force_power = {terraformOption.force_power}," +
                            $"explode_strength = {terraformOption.explode_strength}," +
                            $"damageBuildings = {terraformOption.damageBuildings}," +
                            $"setFire = {terraformOption.setFire}," +
                            $"addBurned = {terraformOption.addBurned}," +
                            $"shake_intensity = 1f," +
                            $"damage = {terraformOption.damage},"

                            );
                        AddReqCodeToBlock(src, type, terraformOption.id);
                    }
                    src.Append("\n\t\t}\n\t}\n}");
                    break;
                //NEW_MAJOR_GEN_HERE
                default:
                    throw new InvalidOperationException($"Unrecognized lexeme: {type.lexeme}");
            }
            
            
            
        }

        //override
        //private void GenerateCode(string objType, Stmt.Var current)
        //{
        //    if (!src.ToString().Contains("\n\tpublic static void init() \n{"))
        //    {
        //        src.Append("\n\tpublic static void init() \n{");
        //    }
        //    if (objType.Equals("TERRAFORM"))
        //    {
                
        //        if (current.type.type == TokenType.ID) 
        //        {
                    
        //            AddBlockId(src, EvaluateExpr(current.value), objType); 
        //        }
        //        else
        //        {
        //            src.AppendLine($"{current.type.lexeme.ToLower()} = {EvaluateExpr(current.value)}");
        //        }
        //        FinalizeBlock(objType);
        //    }
            

        //}

        
        //// Call this method explicitly after processing all attributes
        //private void FinalizeBlock(string objType)
        //{
            
        //    if (objType.Equals("TERRAFORM"))
        //    {
        //        if (count == 1)  // Ensure block is closed only after all vars
        //        {
        //            src.AppendLine("});");
        //            src.AppendLine($"// Block End {objType}");
        //            count = 0;
        //        }
        //    }
            
        //}

        private void CompileToFile(Token type)
        {
            if (type.lexeme.Equals("TRAITS"))
            {
                GenerateCode(type);
                File.WriteAllText("C:/Users/Admin/Desktop/fart.cs", src.ToString());
                FormatCode("C:/Users/Admin/Desktop/fart.cs");


            }
            if (type.lexeme.Equals("EFFECTS"))
            {
                GenerateCode(type);
                File.WriteAllText("C:/Users/Admin/Desktop/doodoo.cs", src.ToString());
                FormatCode("C:/Users/Admin/Desktop/doodoo.cs");
            }

            if (type.lexeme.Equals("PROJECTILES"))
            {
                GenerateCode(type);
                File.WriteAllText("C:/Users/Admin/Desktop/poopoo.cs", src.ToString());
                FormatCode("C:/Users/Admin/Desktop/poopoo.cs");
            }
            if (type.lexeme.Equals("TERRAFORM"))
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

                    
                    powerFuncs += $"public static bool {effect.id}(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)" +
                                  "\n{" +
                                  "\n\tif (pTarget != null)" +
                                  "\n\t{" +
                                  ApplyCombinations(effect.combinations) +
                                  SpawnEffectCode(effect) +
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
                else if (projectiles.TryGetValue(effectKey, out WrldBxProjectile projectile))
                {
                    powerFuncs += $"public static bool {projectile.id}(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)" +
                                  "\n{" +
                                  "\n\tif (pTarget != null)" +
                                  "\n\t{" + 
                                  ApplyCombinations(projectile.combinations) +
                                  SpawnProjectileCode(projectile) +
                                  "\n\t\treturn true;" +
                                  "\n\t}" +
                                  "\n\t\treturn false;" +
                                  "\n}";

                    mainAttackFunc +=
                        $"\n\t\tif (Toolbox.randomChance({projectile.chance}))" +
                        "\n\t\t{" +
                        $"\n\t\t\t{projectile.id}(pSelf, pTarget, pTile);" +
                        "\n\t\t}";
                }
                else
                {
                    WrldBxScript.Warning($"Could not find {effectKey}" +
                                         $" in your effects/projectiles. FAILED " +
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

        private string ApplyCombinations(List<string> combinations)
        {
            StringBuilder sb = new StringBuilder();
            if (combinations != null && combinations.Count != 0)
            {
                foreach (string combination in combinations)
                {
                    if (effects.TryGetValue(combination, out WrldBxEffect effect))
                    {
                        sb.Append(SpawnEffectCode(effect));
                    }
                    if (projectiles.TryGetValue(combination, out WrldBxProjectile projectile))
                    {
                        sb.Append(SpawnProjectileCode(projectile));
                    }
                    else
                    {
                        //do noting for now
                    }
                }

                return sb.ToString();
            }

            return "";
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
            if ((type.lexeme.Equals("TERRAFORM")))
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
            if ((type.Equals("TERRAFORM")))
            {
                sb.Append("AssetManager.terraform.add(new TerraformOptions\n\t{");
                sb.Append("\t\t\nid = " + InQoutes(name.ToString() + ","));
            }
        }

        private string SpawnProjectileCode(WrldBxProjectile projectile)
        {
            return "Vector2Int pos = pTile.pos;" +
                   "float pDist = Vector2.Distance(pTarget.currentPosition, pos);" +
                   "Vector3 newPoint = Toolbox.getNewPoint(pSelf.currentPosition.x, pSelf.currentPosition.y, (float)pos.x, (float)pos.y, pDist, true);" +
                   "Vector3 newPoint2 = Toolbox.getNewPoint(pTarget.currentPosition.x, pTarget.currentPosition.y, (float)pos.x, (float)pos.y, pTarget.a.stats[S.size], true);" +
                   $"EffectsLibrary.spawnProjectile({InQoutes(projectile.id)}, newPoint, newPoint2, 0.0f);";
        }
        private string SpawnEffectCode(WrldBxEffect effect)
        {
            return $"EffectsLibrary.spawn({InQoutes(effect.id)}, " +
                   $"{(effect.spawnsOnTarget ? "pTarget.a.currentTile" : "pSelf.a.currentTile")}, null, null, 0f, -1f, -1f);";
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
