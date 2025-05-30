﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WrldBxScript
{
    public class StatusesCodeGenerator : ICodeGenerator
    {
        private readonly Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> _repositories;

        // Constructor that accepts repositories
        public StatusesCodeGenerator(Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> repositories)
        {
            _repositories = repositories;
        }
        public void GenerateCode(StringBuilder src, string modname)
        {
            src.AppendLine("\tpublic static void init() \n\t{");
            
            foreach (WrldBxStatus status in _repositories["STATUSES"].GetAll.Cast<WrldBxStatus>())
            {


                AddBlockId(src, status.id);
                AddIfHasValue(src, status.health, "health", status.id);
                AddIfHasValue(src, status.damage, "damage", status.id);
                AddIfHasValue(src, status.critChance, "crit_chance", status.id);
                AddIfHasValue(src, status.range, "range", status.id);
                AddIfHasValue(src, status.attackSpeed, "attack_speed", status.id);
                AddIfHasValue(src, status.dodge, "dodge", status.id);
                AddIfHasValue(src, status.accuracy, "accuracy", status.id);
                AddIfHasValue(src, status.scale, "scale", status.id, true);
                AddIfHasValue(src, status.intelligence, "intelligence", status.id);
                AddIfHasValue(src, status.warfare, "warfare", status.id);
                AddIfHasValue(src, status.stewardship, "stewardship", status.id);
                src.AppendLine(HandlePath(status));

                AddReqCodeToBlock(src, status.id);



            }

            src.Append("\n\t}");
            src.Append(Constants.STATUSESEOF);
        }
        private string ToStatString(string nameP, string type)
        {
            return "\t\t\n" + ReplaceWhiteSpace(nameP.ToLower()) + ".base_stats[S." + type + "] += ";
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
            src.AppendLine($"\t\tvar {name} = AssetManager.effects_library.add(new EffectAsset {{");
            src.AppendLine($"\t\t\tid = {InQuotes(name.ToString())}");
        }

        public void AddReqCodeToBlock(StringBuilder src, object name, string appendage = null)
        {
            src.Append($"localizeStatus({name}.id, {name}.id, {name}.description);");
            src.Append($"AssetManager.status.add({name});"); 
        }

        private string HandlePath(WrldBxStatus status)
        {
            
            status.pathIcon = status.pathIcon.ToString().Trim('"');
            if (!System.IO.File.Exists(status.pathIcon.ToString()))
            {
                //give dummy path later 
                WrldBxScript.Warning("Path was not found using default");
                return $"{status.id}.path_icon = \"ui/icons/iconBlessing\";";
            }

            // 5/21/2025, UPDATED TOUSE THE MOD FOLDER TESTING PENDING
            string targetLocation = Path.Combine(WrldBxScript.compiler.OutwardModFolder, "GameResources", "ui", "icons");
            if (!Directory.Exists(targetLocation)) Directory.CreateDirectory(targetLocation);

            string targetPath = System.IO.Path.Combine(targetLocation, System.IO.Path.GetFileName(status.pathIcon.ToString()));

            if (System.IO.File.Exists(targetPath))
            {
                return $"{status.id}.path_icon = \"{status.pathIcon}\";";
            }
            //else
            try
            {
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(status.pathIcon.ToString());
                System.IO.File.Move(status.pathIcon.ToString(), targetPath);
                return $"{status.id}.path_icon = \"ui/icons/{fileNameWithoutExtension}\";";
            }
            catch (Exception ex)
            {

                //TODO: For Error like warning we need to make a debug log that the user can check
                WrldBxScript.Warning($"There was an error moving the files, with path: {status.pathIcon}, using default path");
                return $"{status.id}.path_icon = \"{status.pathIcon}\";";
            }
            

        }

        private string InQuotes(string str) => $"\"{str}\"";
    }
}

