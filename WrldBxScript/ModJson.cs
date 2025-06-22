using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WrldBxScript
{
    public class ModInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("targetGameBuild")]
        public int TargetGameBuild { get; set; }

        [JsonProperty("iconPath")]
        public string IconPath { get; set; }

        public static void Write(string path, string modname)
        {
            ModInfo modjson = new ModInfo
            {
                Name = modname,
                Author = $"{Environment.UserName.ToString()}_{modname}",
                Version = "1.0.0",
                Description = "A mod build by WrldBxCompiler",
                TargetGameBuild = 558,
                IconPath = "icon.png"
            };
            string json = JsonConvert.SerializeObject(modjson, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
