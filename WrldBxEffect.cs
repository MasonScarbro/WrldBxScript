using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public class WrldBxEffect
    {
        private string id;
        private bool spawnsFromActor;
        private bool spawnsOnTarget;

        public WrldBxEffect(string id)
        {
            Console.WriteLine("NEW EFFECT REGISTERED");
            this.id = id;
            this.spawnsFromActor = false;
        }

        public void UpdateStats(Token type, object value)
        {
                Console.WriteLine("Effect " + id + " updated: " + type.lexeme);
                Console.WriteLine("VALUE: " + value);
                if (type.type == TokenType.ID) id = value.ToString();
                if (type.type == TokenType.SPAWNFROMACTOR) spawnsFromActor = bool.Parse(value.ToString());
                if (type.type == TokenType.SPAWNONTARGET) spawnsOnTarget = bool.Parse(value.ToString());
            
        }
    }
}
