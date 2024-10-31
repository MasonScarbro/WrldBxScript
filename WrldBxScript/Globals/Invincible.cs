using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript.Globals
{
    public class Invincible : IGlobal
    {
        public Invincible() { }
        public string TypeAllowance => "Effect";

        public string Call(List<object> arguments)
        {
            if (arguments.Count != 0)
            {
                if (arguments[0] != null)
                {
                    try
                    {
                        return $"pSelf.addStatusEffect(\"invincible\", {Convert.ToDouble(arguments[0])}f);";
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
            //else
            return "";
                  
        }
    }
}
