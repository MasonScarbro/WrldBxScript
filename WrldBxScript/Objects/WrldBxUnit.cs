
using System;
using System.Collections.Generic;
using System.Linq;
// WE WILL SEPERATE RACES AND UNITS, UNITS = MOBS, RACES = RACES
namespace WrldBxScript
{
    public class WrldBxunit : IWrldBxObject
    {
        public string id { get; set; }
        public string template { get; set; }
        public string job { get; set; }
        public bool oceanCreature { get; set; }
        public bool flying { get; set; }
        public bool needFood { get; set; }
        public bool take_items { get; set; }
        public bool use_items { get; set; }
        public string icon { get; set; }
        public string sprite { get; set; }
        public List<object> unit_traits { get; set; }
        //Units NEED base stats, cant be optional
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
        public WrldBxunit(string id)
        {
            this.id = id;
            this.template = "alien_name";
            this.job = "random_move";
            this.oceanCreature = false;
            this.flying = false;
            this.needFood = false;
            this.take_items = false;
            this.use_items = true;
            this.icon = "ui/icons/iconBlessing";
            this.sprite = "NakedManExample";
            this.health = 100;
            this.damage = 5;
            this.critChance = 0;
            this.range = 0;
            this.attackSpeed = 0;
            this.dodge = 0;
            this.accuracy = 0;
            this.scale = 0;
            this.intelligence = 0;    
            this.warfare = 0;
            this.stewardship = 0;


        }

        public void UpdateStats(Token type, object value)
        {
            try
            {
                switch (type.type)
                {
                    case TokenType.PATH:
                        if (type.lexeme.Equals("TEXTURE") || type.lexeme.Equals("SPRITE"))
                        {
                            sprite = value.ToString();
                        }
                        else
                        {
                            icon = value.ToString();
                        }
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
                    case TokenType.TEMPLATE:
                        template = value.ToString();
                        break;
                    case TokenType.JOB:
                        job = value.ToString();
                        break;
                    case TokenType.OCEANCREATURE:
                        oceanCreature = bool.Parse(value.ToString());
                        break;
                    case TokenType.FLYING:
                        flying = bool.Parse(value.ToString());
                        break;
                    case TokenType.NEEDFOOD:
                        needFood = bool.Parse(value.ToString());
                        break;
                    case TokenType.TAKE_ITEMS:
                        take_items = bool.Parse(value.ToString());
                        break;
                    case TokenType.USE_ITEMS:
                        use_items = bool.Parse(value.ToString());
                        break;
                    case TokenType.UNIT_TRAITS:
                        unit_traits = new List<object>();
                        if (value is List<object> list)
                        {
                            unit_traits.AddRange(list.Select(item => item));
                        }
                        else
                        {
                            unit_traits.Add(value);
                        }
                        break;
                    default:
                        throw new CompilerError(type,
                            $"The Keyword {type.lexeme} does not exist within the block");

                }
            }
            catch (CompilerError)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CompilerError(type,
                    $"Tried and failed to convert Value for {type.lexeme} " +
                    $" check what type of value" +
                    $" you should be assigning for the variable," +
                    $" you tried {value} is that right?");
            }
            
        }
    }
}
