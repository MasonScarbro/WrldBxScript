using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrldBxScript.Objects;

namespace WrldBxScript.Generators
{
    public class KingdomCodeGenerator : ICodeGenerator
    {
        //We should probably make this some sort of global shared throught the whole program
        private static readonly HashSet<string> KnownKingdoms = new HashSet<string>(new[] {
            "snakes",
            "snow",
            "snowman",
            "special",
            "super_pumpkin",
            "tornadoes",
            "tumor",
            "turtle",
            "ufo",
            "undead",
            "walkers",
            "wolves",
            "crab",
            "crabzilla",
            "crocodiles",
            "crystals",
            "demons",
            "dog",
            "dragons",
            "druid",
            "dwarf",
            "elf",
            "evil",
            "evilMage",
            "demon"
        });

        private readonly Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> _repositories;

        // Constructor that accepts repositories
        public KingdomCodeGenerator(Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> repositories)
        {
            _repositories = repositories;
        }
        public void GenerateCode(StringBuilder src, string modname)
        {
            src.AppendLine("\tpublic static void init() \n\t{");

            // Add effects-specific generation logic here
            foreach (WrldBxKingdom kingdom in _repositories["KINGDOMS"].GetAll.Cast<WrldBxKingdom>())
            {
                AddBlockId(src, kingdom.id);
                src.Append($"{kingdom.id}.mobs = {kingdom.isMob.ToString().ToLower()};" +
                    $"{kingdom.id}.addTag({InQuotes(kingdom.id)})");
                HandleKingdomTerms(kingdom, src);

                AddReqCodeToBlock(src, kingdom.id);
            }
            src.Append("\n\t\t}\n\t}\n}");
        }

        private void HandleKingdomTerms(WrldBxKingdom kingdom, StringBuilder src)
        {

            if (kingdom.enemy !=  null)
            {
                foreach (object enemy in kingdom.enemy)
                {
                    if (IsKnownKingdom(enemy.ToString()))
                    {
                        src.Append($"{kingdom.id}.addFriendlyTag({InQuotes("SK." + enemy.ToString())})");
                    }
                    else if (_repositories["KINGDOMS"].Exists(enemy.ToString()))
                    {
                        src.Append($"{kingdom.id}.addFriendlyTag({InQuotes(enemy.ToString())})");
                    }
                    else
                    {
                        WrldBxScript.Warning($"Kingdom: {enemy} does not exists thus was not added");
                    }
                    
                }
            }
            if (kingdom.friendly != null)
            {
                foreach (object friend in  kingdom.friendly)
                {
                    if (IsKnownKingdom(friend.ToString()))
                    {
                        src.Append($"{kingdom.id}.addFriendlyTag({InQuotes("SK." + friend.ToString())})");
                    }
                    else if (_repositories["KINGDOMS"].Exists(friend.ToString()))
                    {
                        src.Append($"{kingdom.id}.addFriendlyTag({InQuotes(friend.ToString())})");
                    }
                    else
                    {
                        WrldBxScript.Warning($"Kingdom: {friend} does not exists thus was not added");
                    }
                }
            }
        }

        public bool IsKnownKingdom(string kingdom)
        {
            return KnownKingdoms.Contains(kingdom);
        }

        public void AddBlockId(StringBuilder src, object name)
        {
            src.Append($"KingdomAsset {name} = new KingdomAsset();");
            src.Append($"{name}.id = {InQuotes(name.ToString())};");

        }

        public void AddReqCodeToBlock(StringBuilder src, object name, string appendage = null)
        {
            src.Append($"AssetManager.kingdoms.add({name});" +
                $"MapBox.instance.kingdoms.CallMethod(\"newHiddenKingdom\", {name});");
        }
        private string InQuotes(string str) => $"\"{str}\"";
    }
}
