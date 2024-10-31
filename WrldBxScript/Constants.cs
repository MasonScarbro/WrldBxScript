using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    class Constants
    {
        public const string TRAITSEOF =  @"
        public static void addTraitToLocalizedLibrary(string id, string description) 
        {
            string language = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, 'language') as string;
            Dictionary<string, string> localizedText = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, ''localizedText'') as Dictionary<string, string>;
            localizedText.Add('trait_' + id, id);
            localizedText.Add('trait_' + id + '_info', description);
        }
        " + "\n}" + "\n}";

        public const string STATUSESEOF = @"
        public static void localizeStatus(string id, string name, string description) 
        {
            Dictionary<string, string> localizedText = Reflection.GetField(LocalizedTextManager.instance.GetType(), 
                                                    LocalizedTextManager.instance, 'localizedText') as Dictionary<string, string>;
    
            localizedText.Add(name, id);
            localizedText.Add(description, description);
        }
        " + "\n}" + "\n}";

        public const string WIZRARDRY = "@" +
        "if (Toolbox.randomChance(0.4))" +
        "\n{" +
         "\tMapBox.instance.dropManager.spawn(pTile, \"fire\", 5f, -1f);\r\n" +
         "\tMapBox.instance.dropManager.spawn(pTile, \"acid\", 5f, -1f);" +
         "\t MapBox.instance.dropManager.spawn(pTile, \"fire\", 5f, -1f);" +
        "\n}" +
        "if (Toolbox.randomChance(0.2))" +
        "\n{" +
        "\t\tActionLibrary.addFrozenEffectOnTarget(null, pTarget, null);" +
        "\n}";
    }
}
