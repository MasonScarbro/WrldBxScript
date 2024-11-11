using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript.Globals
{
    public class Shield : IGlobal
    {
        public Shield() { }


        public string Type { get; set; }

        List<string> IGlobal.TypeAllowance => new List<string> { "Effects" };

        public string Call(List<object> arguments)
        {
            if (arguments.Count == 1)
            {
                if (arguments[0] != null)
                {
                    try
                    {
                        return $"pSelf.addStatusEffect(\"shield\", {Convert.ToDouble(arguments[0])}f);";
                    }
                    catch (InvalidCastException e)
                    {
                        WrldBxScript.Warning($"If You are seeing this message" +
                            $" it is because you entered an incorrect" +
                            $" parameter for global call @invinible," +
                            $" you entered: {arguments[0]} It should be a number");
                    }
                }
            }
            if (arguments.Count > 1)
            {
                if (arguments[0] != null && arguments[1] != null)
                {
                    try
                    {
                        return $"{(arguments[1].ToString() == "self" ? "pSelf" : "pTarget")}.addStatusEffect(\"shield\", {Convert.ToDouble(arguments[0])}f);";
                    }
                    catch (InvalidCastException e)
                    {
                        WrldBxScript.Warning($"If You are seeing this message" +
                            $" it is because you entered an incorrect" +
                            $" parameter for global call @shield," +
                            $" you entered: {arguments[0]} It should be a number");
                    }
                }
            }
            //else blank
            return "";
        }

        public void SetType(string type)
        {
            Type = type;
        }
    }
}
