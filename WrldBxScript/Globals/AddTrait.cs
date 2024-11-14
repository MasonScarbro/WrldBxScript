using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript.Globals
{
    public class AddTrait : IGlobal
    {
        private Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> _repositories;

        private List<string> uniqueAttributes = new List<string>
        {
            "evil",
            "poison_immune",
            "fire_proof",
            "acid_proof",
            "burning_feet",
            "shiny",
            "fire_blood",
            "immortal",
            "regeneration",
            "blessed",
            "agile",
            "weightless",
            "fast",
            "energized",
            "light_lamp",
            "genius",
            "freeze_proof",
            "tough",
            "strong_minded",
            "wise",
            "bloodlust",
            "cold_aura",
            "nightchild",
            "moonchild",
            "giant",
            "strong",
            "fat",
            "ambitious",
            "pyromaniac",
            "veteran",
            "acid_touch",
            "acid_blood"
        };

        public AddTrait(Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> repositories)
        {
            _repositories = repositories;
            Type = "Effect_Appendage";
        }
        public List<string> TypeAllowance => new List<string>{"Effects", "Units"};

        public string Type { get; set; }

        

        public string Call(List<object> arguments)
        {
            if (arguments.Count != 0)
            {
                if (arguments[0] != null)
                {
                    try
                    {
                        if (uniqueAttributes.Contains(arguments[0].ToString()) ||
                            _repositories["TRAITS"].Exists(arguments[0].ToString()))
                        {
                            if (Type.Equals("Unit_Appendage"))
                            {


                                return $"AssetManager.actor_library.CallMethod(\"addTrait\", \"{arguments[0].ToString()}\");";
                            }
                            var target = arguments.Count > 1 && arguments[1]?.ToString() == "self" ? "pSelf" : "pTarget";
                            return $"{target}.a.addTrait({arguments[0].ToString()})";
                            
                        }
                        
                    }
                    catch (Exception e)
                    {
                        WrldBxScript.Warning($"If You are seeing this message " +
                            $"It is because addTrait is experimental and something" +
                            $"went wrong Error: {e}");
                    }
                }
                
            }
            //else blank
            WrldBxScript.Warning($"We could not find the trait {arguments[0].ToString()} in your traits or an existing game trait");
            return "";
        }

        public void SetType(string type)
        {
            Type = type;
        }
    }
}
