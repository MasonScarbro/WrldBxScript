using Microsoft.Build.Framework.XamlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript.Globals
{
    internal class DamageWorld : IGlobal
    {
        public DamageWorld() { Type = "Effect_Appendage"; }
        
        public List<string> TypeAllowance => new List<string> { "Effects" };

        public string Type { get; set; }

        public string Call(List<object> arguments)
        {
            if (arguments.Count == 2)
            {
                if (arguments[0] != null)
                {
                    try
                    {
                        return $"MapAction.damageWorld(pTarget.currentTile{NeighbourChainMacro(int.Parse(arguments[1].ToString()))}, 8, AssetManager.terraform.get(\"{arguments[0]}\"), null);";
                    }
                    catch (InvalidCastException e)
                    {
                        WrldBxScript.Warning($"If You are seeing this message" +
                            $" it is because you entered an incorrect" +
                            $" parameter for the offset," +
                            $" you entered: {arguments[1]}, It should be a number");
                    }
                }
            }
            else if (arguments.Count > 2)
            {
                if (arguments[0] != null)
                {
                    try
                    {
                        return $"MapAction.damageWorld({(arguments[2].ToString() == "self" ? "pSelf" : "pTarget")}.currentTile{NeighbourChainMacro(int.Parse(arguments[1].ToString()))}, 8, AssetManager.terraform.get(\"{arguments[0]}\"), null);";
                    }
                    catch (InvalidCastException e)
                    {
                        WrldBxScript.Warning($"If You are seeing this message" +
                            $" it is because you entered an incorrect" +
                            $" parameter for the offset," +
                            $" you entered: {arguments[1]}, It should be a number");
                    }
                }
            }
            //else
            return "";

        }

        public void SetType(string type)
        {
            Type = type;
        }

        private string NeighbourChainMacro(int value)
        {
            if (value <= 3)
                return $".neighbours[{value}]";

            int quotient = value / 3;
            int remainder = value % 3;
            StringBuilder chain = new StringBuilder();
            for (int i = 0; i < quotient; i++)
            {
                chain.Append(".neighbours[3]");
            }
            if (remainder > 0) chain.Append($".neighbours[{remainder}]");

            return chain.ToString();

        }
    }
}
