using System;
using System.Collections.Generic;
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
                src.AppendLine($"{status.id}.path_icon = {InQuotes(status.pathIcon)};");

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

        private string InQuotes(string str) => $"\"{str}\"";
    }
}

