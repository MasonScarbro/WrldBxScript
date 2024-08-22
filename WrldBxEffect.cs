using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public class WrldBxEffect
    {
        public string id;
        public int chance;
        public bool spawnsFromActor;
        public bool spawnsOnTarget;
        public bool IsAttack;

        public WrldBxEffect(string id)
        {
            Console.WriteLine("NEW EFFECT REGISTERED");
            this.id = id;
            this.spawnsFromActor = false;
            this.spawnsOnTarget = true;
            this.IsAttack = true;
            this.chance = 1;
        }

        public void UpdateStats(Token type, object value)
        {
                Console.WriteLine("Effect " + id + " updated: " + type.lexeme);
                Console.WriteLine("VALUE: " + value);
                if (type.type == TokenType.ID) id = value.ToString();
                if (type.type == TokenType.SPAWNFROMACTOR) spawnsFromActor = bool.Parse(value.ToString());
                if (type.type == TokenType.SPAWNONTARGET) spawnsOnTarget = bool.Parse(value.ToString());
                if (type.type == TokenType.ISATTK) IsAttack = bool.Parse(value.ToString());

        }
    }
}
