using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WrldBxScript
{
    
    public class EffectsCodeGenerator : ICodeGenerator
    {
        private readonly Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> _repositories;

        // Constructor that accepts repositories
        public EffectsCodeGenerator(Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> repositories)
        {
            _repositories = repositories;
        }
        public void GenerateCode(StringBuilder src, string modname)
        {
            src.AppendLine("\tpublic static void init() \n\t{");
            
            // Add effects-specific generation logic here
            foreach (WrldBxEffect effect in _repositories["EFFECTS"].GetAll.Cast<WrldBxEffect>())
            {
                AddBlockId(src, effect.id);
                src.AppendLine($"\t\tsprite_path = {InQuotes(effect.sprite_path)},");
                src.AppendLine($"\t\ttime_between_frames = {effect.time_between_frames},");
                src.AppendLine($"\t\tdraw_light_area = {effect.draw_light_area},");
                src.AppendLine($"\t\tdraw_light_size = {effect.draw_light_size},");
                src.AppendLine($"\t\tlimit = {effect.limit},");

                AddReqCodeToBlock(src, effect.id);
            }

            src.AppendLine("\t}\n}\n}");
        }

        public void AddBlockId(StringBuilder src, object name)
        {
            src.AppendLine($"\t\tvar {name} = AssetManager.effects_library.add(new EffectAsset {{");
            src.AppendLine($"\t\t\tid = {InQuotes(name.ToString())}");
        }

        public void AddReqCodeToBlock(StringBuilder src, object name, string appendage = null)
        {
            src.AppendLine("\t\t});");
            src.AppendLine($"World.world.stackEffects.CallMethod(add, {InQuotes(name.ToString())});");
        }

        private string InQuotes(string str) => $"\"{str}\"";
    }
}
