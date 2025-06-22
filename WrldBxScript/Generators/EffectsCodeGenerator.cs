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
                src.AppendLine(HandlePath(effect, "Sprite"));
                src.AppendLine($"\t\ttime_between_frames = {effect.time_between_frames},");
                src.AppendLine($"\t\tdraw_light_area = {effect.draw_light_area},");
                src.AppendLine($"\t\tdraw_light_size = {effect.draw_light_size}f,");
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

        private string HandlePath(WrldBxEffect effect, string type)
        {
            
            if (type.Equals("Sprite"))
            {
                effect.sprite_path = effect.sprite_path.ToString().Trim('"');
                if (!System.IO.Directory.Exists(effect.sprite_path.ToString()))
                {
                    //give dummy path later 
                    WrldBxScript.Warning("Path was not found using default", effect);
                    return $"texture_path = \"NakedMan\",";

                }
                string spriteFolderName = System.IO.Path.GetFileName(effect.sprite_path.ToString());
                // 5/21/2025, UPDATED TOUSE THE MOD FOLDER TESTING PENDING
                string targetLocation = System.IO.Path.Combine(WrldBxScript.compiler.OutwardModFolder, "GameResources", "effects");

                if (System.IO.Directory.Exists(targetLocation))
                {
                    return $"texture_path = \"effects/{spriteFolderName}\",";
                }

                //else
                try
                {
                    CopyDirectory(effect.sprite_path.ToString(), targetLocation);
                    return $"texture_path = \"effects/{spriteFolderName}\",";
                }
                catch (Exception ex)
                {

                    //TODO: For Error like warning we need to make a debug log that the user can check
                    WrldBxScript.Warning($"There was an error moving the files, with path: {effect.sprite_path}, using default path");
                    return $"texture_path = \"effects/{effect.sprite_path}\",";
                }
            }
            //else
            return "";

        }
        private void CopyDirectory(string sourceDir, string targetDir)
        {
            System.IO.Directory.CreateDirectory(targetDir);
            var files = System.IO.Directory.GetFiles(sourceDir, "*.*", System.IO.SearchOption.TopDirectoryOnly);

               
            foreach (var file in files)
            {
                string targetFilePath = System.IO.Path.Combine(targetDir, System.IO.Path.GetFileName(file));
                System.IO.File.Copy(file, targetFilePath, true);
            }
          

        }

    }
}
