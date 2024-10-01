using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public class WrldBxTrait
    {
        public string id;
        public double health;
        public double damage;
        public double critChance;
        public double range;
        public double attackSpeed;
        public double dodge;
        public double accuracy;
        public double scale;
        public double intelligence;
        public double warfare;
        public double stewardship;
        public string pathIcon;
        public string effectName;
        public WrldBxTrait(string id)
        {
            Console.WriteLine("NEW Trait REGISTERED");
            this.id = id;
            this.pathIcon = "ui/icons/iconBlessing";
        }

        public void UpdateStats(Token type, object value)
        {

            Console.WriteLine("Effect " + id + " updated: " + type.lexeme);
            Console.WriteLine("VALUE: " + value);
            switch(type.type)
            {
                case TokenType.ID:
                
                this.id = value.ToString();
                break;

                case TokenType.HEALTH:
                health = Convert.ToDouble(value.ToString());
                break;

                case TokenType.DAMAGE:
                damage = Convert.ToDouble(value.ToString());
                break;

                case TokenType.CRIT_CHANCE:
                critChance = Convert.ToDouble(value.ToString());
                break;

                case TokenType.RANGE:
                range = Convert.ToDouble(value.ToString());
                break;

                case TokenType.ATTACK_SPEED:
                attackSpeed = Convert.ToDouble(value.ToString());
                break;

                case TokenType.DODGE:
                dodge = Convert.ToDouble(value.ToString());
                break;

                case TokenType.ACCURACY:
                accuracy = Convert.ToDouble(value.ToString());
                break;

                case TokenType.SCALE:
                scale = Convert.ToDouble(value.ToString());
                break;

                case TokenType.INTELIGENCE:
                intelligence = Convert.ToDouble(value.ToString());
                break;

                case TokenType.WARFARE:
                warfare = Convert.ToDouble(value.ToString());
                break;

                case TokenType.STEWARDSHIP:
                stewardship = Convert.ToDouble(value.ToString());
                break;

                case TokenType.PATH:
                pathIcon = value.ToString();
                break;

                case TokenType.POWER: 
                effectName = value.ToString();
                break;

                default:
                Console.WriteLine("Unknown TokenType: " + type.type);
                break;
            }

        }
    }
}
