﻿using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework.XamlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrldBxScript.Globals;


namespace WrldBxScript
{
    public class TraitsCodeGenerator : ICodeGenerator
    {
        private readonly Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> _repositories;
        private readonly Dictionary<string, object> _globals;
        // Constructor that accepts repositories
        public TraitsCodeGenerator(Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> repositories, Dictionary<string, object> globals)
        {
            _globals = globals;
            _repositories = repositories;
        }
        public void GenerateCode(StringBuilder src, string modname)
        {
            src.AppendLine("\tpublic static void init() \n\t{");
            var funcs = new StringBuilder();
            foreach (WrldBxTrait trait in _repositories["TRAITS"].GetAll.Cast<WrldBxTrait>())
            {


                AddBlockId(src, trait.id);
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
                src.AppendLine(HandlePath(trait, "Icon"));
                if (trait.effectName != null)
                {
                    src.AppendLine($"{trait.id}.action_attack_target = new AttackAction({trait.id}Attack);");
                    src.AppendLine($"{trait.id}.action_special_effect = (WorldAction)Delegate.Combine({trait.id}.action_special_effect, new WorldAction({trait.id}Special));");
                }
                //NOTE: test if this works if you never put any powers ^
                AddReqCodeToBlock(src, trait.id, $"addTraitToLocalizedLibrary({trait.id}.id, {InQuotes(trait.desc)});");

                funcs.Append(BuildTraitPowerFunctions(trait, src));

            }

            src.Append("\n\t}");
            src.Append(funcs);
            src.Append(Constants.TRAITSEOF);
        }





        private string BuildTraitPowerFunctions(WrldBxTrait trait, StringBuilder osb)
        {

            string powerFuncs = "";
            string mainAttackFunc =
                $"public static bool {trait.id}Attack(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)" +
                "\n{" +
                "\n\tif (pTarget != null)" +
                "\n\t{";

            string mainSpecialFunc =
                $"public static bool {trait.id}Special(BaseSimObject pSelf, WorldTile pTile)" +
                "\n{" +
                "\n\tif (pSelf.a != null)" +
                "\n\t{";


            foreach (string effectKey in trait.effectName)
            {

                if (TryGetEffect(effectKey, out WrldBxEffect effect))
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
                            $"\n\t\tif (Randy.randomChance({effect.chance}))" +
                            "\n\t\t{" +
                            $"\n\t\t\t{effect.id}(pSelf, pTarget, pTile);" +
                            "\n\t\t}";

                    }
                    else
                    {
                        mainSpecialFunc +=
                            $"\n\t\tif (Randy.randomChance({effect.chance}))" +
                            "\n\t\t{" +
                            $"\n\t\t\t{effect.id}(pSelf, pTarget, pTile);" +
                            "\n\t\t}";
                    }
                }
                else if (TryGetProjectile(effectKey, out WrldBxProjectile projectile))
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
                    if (osb.ToString().Contains(powerFuncs))
                    {
                        osb.Replace(powerFuncs, powerFuncs); //this seems stupid but recompiling on the same build needs this so that it doesnt just add it
                    }
                    mainAttackFunc +=
                        $"\n\t\tif (Randy.randomChance({projectile.chance}))" +
                        "\n\t\t{" +
                        $"\n\t\t\t{projectile.id}(pSelf, pTarget, pTile);" +
                        "\n\t\t}";
                }
                else
                {
                    WrldBxScript.Warning($"Could not find {effectKey}" +
                                         $" in your effects/projectiles. FAILED " +
                                         $"to build power for it", trait);

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

        private string ApplyCombinations(List<object> combinations)
        {
            StringBuilder sb = new StringBuilder();
            if (combinations != null && combinations.Count != 0)
            {
                foreach (object combination in combinations)
                {
                    if (TryGetEffect(combination.ToString(), out WrldBxEffect effect))
                    {
                        sb.Append(SpawnEffectCode(effect));
                    }
                    else if (TryGetProjectile(combination.ToString(), out WrldBxProjectile projectile))
                    {
                        sb.Append(SpawnProjectileCode(projectile));
                    }
                    else if (TryGetGlobals(combination, out string relatedSrc))
                    {
                        sb.Append(relatedSrc);
                    }
                }

                return sb.ToString();
            }

            return "";
        }


        private string SpawnProjectileCode(WrldBxProjectile projectile)
        {
            // TODO: amount needs to be set to the projectile.amount
            return $@"
            Vector2 Pos = default;
            Vector3 Start = Pos == default ? pSelf.current_position : Pos;
            int amount = 1;
            float tZ = 0f; 
            float pZ = 0.25f;
            if (pTarget.isInAir())
            {{
                tZ = pTarget.getHeight();
            }}
            for (int i = 0; i < amount; i++)
            {{
                Vector3 tTargetPos = pTarget.current_tile.posV3;
                tTargetPos.x += Randy.randomFloat(-(pTarget.stats[""size""] + 1f), pTarget.stats[""size""] + 1f);
                tTargetPos.y += Randy.randomFloat(-pTarget.stats[""size""], pTarget.stats[""size""]);
                World.world.projectiles.spawn(pSelf, pTarget, {InQuotes(projectile.id)}, Start, tTargetPos, tZ, pZ);
            }}";
        }
        private string SpawnEffectCode(WrldBxEffect effect)
        {
            return $"EffectsLibrary.spawn({InQuotes(effect.id)}, " +
                   $"{(effect.spawnsOnTarget ? "pTarget.a.currentTile" : "pSelf.a.currentTile")}, null, null, 0f, -1f, -1f);";
        }

        private bool TryGetEffect(string effectKey, out WrldBxEffect effect)
        {
            // Initialize the effect to null to satisfy the out parameter requirement.
            effect = null;

            // Try to get the object from the repository.
            if (_repositories["EFFECTS"].GetObject(effectKey) is WrldBxEffect foundEffect)
            {
                // If foundEffect is not null, assign it to effect and return true.
                effect = foundEffect;
                return true;
            }

            // If not found, return false.
            return false;
        }

        private bool TryGetProjectile(string effectKey, out WrldBxProjectile projectile)
        {
            // Initialize the projectile to null to satisfy the out parameter requirement.
            projectile = null;

            // Try to get the object from the repository.
            if (_repositories["PROJECTILES"].GetObject(effectKey) is WrldBxProjectile foundProjectile)
            {
                // If foundProjectile is not null, assign it to projectile and return true.
                projectile = foundProjectile;
                return true;
            }

            // If not found, return false.
            return false;
        }

        private bool TryGetGlobals(object combination, out string src)
        {
            src = "";
            if (combination is ValueTuple<object, List<object>> tuple) // Checking if it's a tuple
            {
                string functionName = tuple.Item1.ToString();
                var arguments = tuple.Item2;

                // Process function call with name and args
                Console.WriteLine($"Function: {functionName}, Arguments: {string.Join(", ", arguments)}");
                if (_globals.TryGetValue(functionName, out object global))
                {
                    if (global is IGlobal globalObj)
                    {
                        src += globalObj.Call(arguments);
                        return true;
                    }
                }
                //else
                WrldBxScript.Warning($"Could not find {functionName} In stored Globals It has been skipped");
            }
            else
            {
                //functionality if its just a identifier (no '()')
                if (_globals.TryGetValue(combination.ToString(), out object global))
                {
                    if (global is string constant)
                    {
                        src += constant;
                        return true;
                    }
                }
            }
            return false;
        }
        private string ToStatString(string nameP, string type)
        {
            return "\t\t\n" + ReplaceWhiteSpace(nameP) + ".base_stats[S." + type + "] += ";
        }
        private string ToParaCase(string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
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
        private void AddIfHasValue<T>(StringBuilder sb, T? value, string statName, string id, bool divideBy100 = false) where T : struct
        {
            if (value.HasValue)
            {
                string statValue = divideBy100 ? (Convert.ToDouble(value.Value) / 100).ToString() : value.ToString();
                sb.AppendLine($"{ToStatString(id, statName)}{statValue};");
            }
        }

        public void AddBlockId(StringBuilder src, object name)
        {
            src.Append($"\t\t\nActorTrait {name} = new ActorTrait();");
            src.Append($"\t\t\n{name}.id = {InQuotes(name.ToString())};");
        }

        public void AddReqCodeToBlock(StringBuilder src, object name, string appendage = null)
        {
            src.Append("\t\t\nAssetManager.traits.add(" + name.ToString() + ");");
            src.Append($"\t\t\n{name.ToString()}.unlock();\n");
            src.Append(appendage);
        }

        private string HandlePath(WrldBxTrait trait, string type)
        {
            if (type.Equals("Icon"))
            {
                trait.pathIcon = trait.pathIcon.ToString().Trim('"'); // For some reason the other handlepaths dont need this i dont think
                if (!System.IO.File.Exists(trait.pathIcon.ToString()))
                {
                    //give dummy path later 
                    WrldBxScript.Warning("Path was not found using default", trait);
                    return $"{trait.id}.path_icon = \"ui/icons/iconBlessing\";";
                }

                // 5/21/2025, UPDATED TOUSE THE MOD FOLDER TESTING PENDING
                string targetLocation = System.IO.Path.Combine(WrldBxScript.compiler.OutwardModFolder, "GameResources", "ui", "icons");

                if (!System.IO.Directory.Exists(targetLocation)) 
                    System.IO.Directory.CreateDirectory(targetLocation);

                string targetPath = System.IO.Path.Combine(targetLocation, System.IO.Path.GetFileName(trait.pathIcon.ToString()));

                if (System.IO.File.Exists(targetPath))
                {
                    return $"{trait.id}.path_icon = \"{trait.pathIcon}\";";
                }
                //else
                try
                {
                    string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(trait.pathIcon.ToString());
                    System.IO.File.Move(trait.pathIcon.ToString(), targetPath);
                    return $"{trait.id}.path_icon = \"ui/icons/{fileNameWithoutExtension}\";";
                }
                catch (Exception ex)
                {

                    //TODO: For Error like warning we need to make a debug log that the user can check
                    WrldBxScript.Warning($"There was an error moving the files, with path: {trait.pathIcon}, using default path");
                    return $"{trait.id}.path_icon = \"{trait.pathIcon}\";";
                }
            }
            
            //else
            return "";

        }
        

        private string InQuotes(string str) => $"\"{str}\"";
    }
}
