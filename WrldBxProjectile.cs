using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    class WrldBxProjectile
    {
        public string id;
        public double chance;
        

        public WrldBxProjectile(string id)
        {
            Console.WriteLine("NEW EFFECT REGISTERED");
            this.id = id;
            this.chance = 1;
        }

        public void UpdateStats(Token type, object value)
        {
            Console.WriteLine("Projectile " + id + " updated: " + type.lexeme);
            Console.WriteLine("VALUE: " + value);
            if (type.type == TokenType.ID) id = value.ToString();

        }
    }
}
