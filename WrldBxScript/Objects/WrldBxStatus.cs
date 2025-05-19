using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer.Collaboration;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public class WrldBxStatus : IWrldBxObject
    {
        public string id { get; set; }
        public double? health;
        public double? damage;
        public double? critChance;
        public double? range;
        public double? attackSpeed;
        public double? dodge;
        public double? accuracy;
        public double? scale;
        public double? intelligence;
        public double? warfare;
        public double? stewardship;
        public string pathIcon;
        public string desc;

        public WrldBxStatus(string id)
        {
            Console.WriteLine("NEW Status REGISTERED");
            this.id = id;
            this.pathIcon = "ui/icons/iconBlessing";
            this.desc = "Bruh You Forgot to add add a Description, BAAAAKAAAA!!! ;/";
        }

        public void UpdateStats(Token type, object value)
        {
            Console.WriteLine("Trait " + id + " updated: " + type.lexeme);
            Console.WriteLine("VALUE: " + value);
            try
            {
                switch (type.type)
                {
                    case TokenType.ID:

                        id = value.ToString();
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

                    case TokenType.DESC:
                        desc = value.ToString();
                        break;

                  ;

                    default:
                        throw new CompilerError(type,
                            $"The Keyword {type.lexeme} does not exist within the PROJECTILES block");

                }
            }
            catch (CompilerError)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CompilerError(type,
                    $"Tried To failed to convert Value for {type.lexeme} " +
                    $" check what type of value" +
                    $" you should be assigning for the variable," +
                    $" you tried {value} is that right?");
            }
        }
    }
}
