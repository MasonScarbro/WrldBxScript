using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public class WrldBxEffect : IWrldBxObject
    {
        public string id { get; set; }
        public double chance;
        public bool spawnsFromActor;
        public bool spawnsOnTarget;
        public bool IsAttack;
        public double time_between_frames;
        public string sprite_path;
        public bool draw_light_area;
        public double draw_light_size;
        public double limit;
        public List<string> combinations;
        public WrldBxEffect(string id)
        {
            Console.WriteLine("NEW EFFECT REGISTERED");
            this.id = id;
            this.spawnsFromActor = false;
            this.spawnsOnTarget = true;
            this.IsAttack = true;
            this.time_between_frames = 0.08;
            this.draw_light_area = true;
            this.draw_light_size = 2;
            this.limit = 100;
            this.sprite_path = "fireball";

            this.chance = 1;
        }

        public void UpdateStats(Token type, object value)
        {

            Console.WriteLine("Effect " + id + " updated: " + type.lexeme);
            Console.WriteLine("VALUE: " + value);
            try
            {
                switch (type.type)
                {
                    case TokenType.ID:
                        id = value.ToString();
                        break;

                    case TokenType.PATH:
                        sprite_path = value.ToString();
                        break;
                    case TokenType.TIMEBETWEENFRAMES:
                        time_between_frames = double.Parse(value.ToString());
                        break;
                    case TokenType.DRAW_LIGHT:
                        draw_light_area = bool.Parse(value.ToString());
                        break;
                    case TokenType.DRAW_LIGHT_SIZE:
                        draw_light_size = double.Parse(value.ToString());
                        break;
                    case TokenType.LIMIT:
                        limit = double.Parse(value.ToString());
                        break;


                    case TokenType.CHANCE:
                        chance = double.Parse(value.ToString());
                        break;

                    case TokenType.SPAWNFROMACTOR:
                        spawnsFromActor = bool.Parse(value.ToString());
                        break;

                    case TokenType.SPAWNONTARGET:
                        spawnsOnTarget = bool.Parse(value.ToString());
                        break;

                    case TokenType.ISATTK:
                        IsAttack = bool.Parse(value.ToString());
                        break;

                    case TokenType.COMBINE:
                        combinations = new List<string>();
                        if (value is List<object> list)
                        {
                            combinations.AddRange(list.Select(item => item.ToString()));
                        }
                        else
                        {
                            combinations.Add(value.ToString());
                        }
                        break;

                    default:
                        throw new CompilerError(type, "This keyword does not exist within the EFFECTS block");

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
