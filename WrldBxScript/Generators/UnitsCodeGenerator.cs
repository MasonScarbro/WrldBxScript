
using Microsoft.Extensions.Primitives;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public class UnitsCodeGenerator : ICodeGenerator
    {
        private readonly Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> _repositories;

        // Constructor that accepts repositories
        public UnitsCodeGenerator(Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> repositories)
        {
            _repositories = repositories;
        }

        public void AddBlockId(StringBuilder src, object name)
        {
            src.Append($"var {name} = AssetManager.actor_library.clone(\"{name}\", \"_mob\");");
            src.Append($"{name}.nameLocale = \"{name}\"");
        }

        public void AddReqCodeToBlock(StringBuilder src, object name, string appendage = null)
        {
            src.Append($"AssetManager.actor_library.add({name})");
            src.Append($"{(appendage == null ? "" : appendage)}");
            src.Append($"Localization.addLocalization({name}.nameLocale, {name}.nameLocale);");
        }

        public void GenerateCode(StringBuilder src, string modname)
        {
            src.AppendLine("\tpublic static void init() \n\t{");

            // Add effects-specific generation logic here
            foreach (WrldBxunit unit in _repositories["UNITS"].GetAll.Cast<WrldBxunit>())
            {
                AddBlockId(src, unit.id);
                src.Append
                    (
                    $"{unit.id}.job = {unit.job};" +
                    $"{unit.id}.nameTemplate = {unit.template};" +
                    $"{unit.id}.needFood = {StringHelpers.ConvertBoolString(unit.needFood)};" +
                    $"{unit.id}.flying = {StringHelpers.ConvertBoolString(unit.flying)};" +
                    $"{unit.id}.oceanCreature = {StringHelpers.ConvertBoolString(unit.oceanCreature)};" +
                    $"{unit.id}.landCreature = {StringHelpers.ConvertBoolString(!unit.oceanCreature)};"+
                    $"{unit.id}.use_items = {StringHelpers.ConvertBoolString(!unit.use_items)};"+
                    $"{unit.id}.take_items = {StringHelpers.ConvertBoolString(!unit.take_items)};" +
                    $"{HandlePath(unit, "Icon")}"+
                    $"{HandlePath(unit, "Sprite")}" +
                    $"{ToStatString(unit.id, "health")}{unit.health};" +
                    $"{ToStatString(unit.id, "accuracy")}{unit.accuracy};" +
                    $"{ToStatString(unit.id, "range")}{unit.range};" +
                    $"{ToStatString(unit.id, "attack_speed")}{unit.attackSpeed};" +
                    $"{ToStatString(unit.id, "crit_chance")}{unit.critChance};" +
                    $"{ToStatString(unit.id, "damage")}{unit.damage};" +
                    $"{ToStatString(unit.id, "dodge")}{unit.dodge};" +
                    $"{ToStatString(unit.id, "scale")}{unit.scale};" +
                    $"{ToStatString(unit.id, "stewardship")}{unit.stewardship};" +
                    $"{ToStatString(unit.id, "warfare")}{unit.warfare};" +
                    $"{ToStatString(unit.id, "intelligence")}{unit.intelligence};" 
                    
                    );
                //Constant
                src.Append
                    (
                    $"{unit.id}.run_to_water_when_on_fire = true;\r\n" +
                    $"{unit.id}.can_edit_traits = true;\r\n" +
                    $"{unit.id}.canBeKilledByDivineLight = false;\r\n" +
                    $"{unit.id}.ignoredByInfinityCoin = false;\r\n" +
                    $"{unit.id}.actorSize = ActorSize.S13_Human;\r\n" +
                    $"{unit.id}.action_liquid = new WorldAction(ActionLibrary.swimToIsland);" +
                    $"{unit.id}.canBeKilledByStuff = true;\r\n" +
                    $"{unit.id}.canBeKilledByLifeEraser = true;\r\n" +
                    $"{unit.id}.canAttackBuildings = true;\r\n" +
                    $"{unit.id}.canBeMovedByPowers = true;\r\n" +
                    $"{unit.id}.canBeHurtByPowers = true;\r\n" +
                    $"{unit.id}.canTurnIntoZombie = false;\r\n" +
                    $"{unit.id}.canBeInspected = true;\r\n" +
                    $"{unit.id}.hideOnMinimap = false;"+
                    $"{unit.id}.skeletonID = \"skeleton_cursed\";\r\n" +
                    $"{unit.id}.zombieID = \"zombie\";"+
                    $"{unit.id}.can_turn_into_demon_in_age_of_chaos = false;\n" +
                    $"{unit.id}.canTurnIntoIceOne = false;\n" +
                    $"{unit.id}.canTurnIntoTumorMonster = false;\n" +
                    $"{unit.id}.canTurnIntoMush = false;\n" +
                    $"{unit.id}.dieInLava = true;\n" +
                    $"{unit.id}.dieOnBlocks = false;\n" +
                    $"{unit.id}.dieOnGround = false;\n" +
                    $"{unit.id}.dieByLightning = true;\n" +
                    $"{unit.id}.damagedByOcean = false;\n" +
                    $"{unit.id}.damagedByRain = false;"
                    );
                AddReqCodeToBlock(src, unit.id);
                src.Append("}\n}");

            }
            

        }
        private string ToStatString(string nameP, string type)
        {
            return "\t\t\n" + StringHelpers.ReplaceWhiteSpace(nameP.ToLower()) + ".base_stats[S." + type + "] += ";
        }

        private string HandlePath(WrldBxunit unit, string type)
        {
            if (type.Equals("Icon"))
            {
                if (!System.IO.File.Exists(unit.icon.ToString()))
                {
                    //give dummy path later 
                    WrldBxScript.Warning("Path was not found using default");
                    return $"{unit.id}.icon = \"ui/icons/iconBlessing\";";
                }

                //For now its a dummy location for the desktop, later we will need to get the workdir
                string targetLocation = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                                         "FakeMod", "main", "GameResources", "ui", "icons");

                System.IO.Directory.CreateDirectory(targetLocation);

                string targetPath = System.IO.Path.Combine(targetLocation, System.IO.Path.GetFileName(unit.icon.ToString()));

                if (System.IO.File.Exists(targetPath))
                {
                    return $"{unit.id}.icon = \"{unit.icon}\";";
                }
                //else
                try
                {
                    string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(unit.icon.ToString());
                    System.IO.File.Move(unit.icon.ToString(), targetPath);
                    return $"{unit.id}.icon = \"ui/icons/{fileNameWithoutExtension}\";";
                }
                catch (Exception ex)
                {

                    //TODO: For Error like warning we need to make a debug log that the user can check
                    WrldBxScript.Warning($"There was an error moving the files, with path: {unit.icon}, using default path");
                    return $"{unit.id}.icon = \"{unit.icon}\";";
                }
            }
            if (type.Equals("Sprite"))
            {
                if (!System.IO.Directory.Exists(unit.sprite.ToString()))
                {
                    //give dummy path later 
                    WrldBxScript.Warning("Path was not found using default");
                    return $"{unit.id}.texture_path = \"NakedMan\";"+
                           $"{unit.id}.animation_swim = \"swim_0,swim_1,swim_2,swim_3\";" +
                           $"{unit.id}.animation_walk = \"walk_0,walk_1,walk_2,walk_3\";";

                }
                string spriteFolderName = System.IO.Path.GetFileName(unit.sprite.ToString());
                //For now its a dummy location for the desktop, later we will need to get the workdir
                string targetLocation = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                                         "FakeMod", "main", "GameResources", "Actors");
                
                if (System.IO.Directory.Exists(targetLocation))
                {
                    return $"{unit.id}.texture_path = \"{spriteFolderName}\";" +
                           $"{unit.id}.animation_swim = \"swim_0,swim_1,swim_2,swim_3\";" +
                           $"{unit.id}.animation_walk = \"walk_0,walk_1,walk_2,walk_3\";";
                }

                //else
                try
                {
                    CopyDirectory(unit.sprite.ToString(), targetLocation);
                    return $"{unit.id}.texture_path = \"{spriteFolderName}\";" +
                           $"{unit.id}.animation_swim = \"swim_0,swim_1,swim_2,swim_3\";" +
                           $"{unit.id}.animation_walk = \"walk_0,walk_1,walk_2,walk_3\";";
                }
                catch (Exception ex)
                {

                    //TODO: For Error like warning we need to make a debug log that the user can check
                    WrldBxScript.Warning($"There was an error moving the files, with path: {unit.icon}, using default path");
                    return $"{unit.id}.texture_path = \"{unit.sprite}\";";
                }
            }
            //else
            return "";
            
        }
        private void CopyDirectory(string sourceDir, string targetDir)
        {
            System.IO.Directory.CreateDirectory(targetDir);
            var files = System.IO.Directory.GetFiles(sourceDir, "*.*", System.IO.SearchOption.TopDirectoryOnly);

            bool hasWalkOrSwimFiles = files.Any(file => System.IO.Path.GetFileName(file).StartsWith("walk_") ||
                                                System.IO.Path.GetFileName(file).StartsWith("swim_"));

            if (!hasWalkOrSwimFiles)
            {
                int walkCount = 0;
                int swimCount = 0;
                foreach (var file in files)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    string targetFileName = fileName;

                    if (walkCount < 4 && !fileName.StartsWith("swim"))
                    {
                        targetFileName = $"walk_{walkCount}";
                        walkCount++;
                    }
                    else if (swimCount < 4 && walkCount > 3)
                    {
                        targetFileName = $"swim_{swimCount}";
                        swimCount++;
                    }

                    string targetFilePath = System.IO.Path.Combine(targetDir, targetFileName);
                    System.IO.File.Copy(file, targetFilePath, true);
                }
            }
            else
            {
                // Simply copy all files without renaming if any walk_ or swim_ files exist
                foreach (var file in files)
                {
                    string targetFilePath = System.IO.Path.Combine(targetDir, System.IO.Path.GetFileName(file));
                    System.IO.File.Copy(file, targetFilePath, true);
                }
            }

        }
    }

}
                    