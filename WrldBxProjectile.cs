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
        public string texture;
        public double speed;
        public bool draw_light_area;
        public double draw_light_size;
        public bool parabolic;
        public double scale;
        public double? animation_speed;
        public bool lookAtTarget;

        public WrldBxProjectile(string id)
        {
            Console.WriteLine("NEW PROJECTILE REGISTERED");
            this.id = id;
            this.chance = 1;
            this.texture = "fireball";
            this.speed = 1;
            this.draw_light_area = true;
            this.draw_light_size = 0.1;
            this.parabolic = false;
            this.scale = 0.1f;
            this.lookAtTarget = true;
            this.animation_speed = null;


        }

        public void UpdateStats(Token type, object value)
        {
            Console.WriteLine("Projectile " + id + " updated: " + type.lexeme);
            Console.WriteLine("VALUE: " + value);
            switch(type.type)
            {
                case TokenType.ID:
                id = value.ToString();
                break;

                case TokenType.PATH:
                    texture = value.ToString();
                break;
                case TokenType.TIMEBETWEENFRAMES:
                    animation_speed = Convert.ToDouble(value.ToString());
                    break;
                case TokenType.DRAW_LIGHT:
                    draw_light_area = bool.Parse(value.ToString());
                    break;
                case TokenType.DRAW_LIGHT_SIZE:
                    draw_light_size = Convert.ToDouble(value.ToString());
                    break;
                case TokenType.CHANCE:
                    chance = Convert.ToDouble(value.ToString());
                    break;
                case TokenType.SPEED:
                    speed = Convert.ToDouble(value.ToString());
                    break;
                case TokenType.FACINGTRGT:
                    lookAtTarget = bool.Parse(value.ToString());
                    break;
                case TokenType.PARABOLIC:
                    parabolic = bool.Parse(value.ToString());
                    break;
                case TokenType.SCALE:
                    scale = Convert.ToDouble(chance.ToString());
                    break;
                default:
                throw new CompilerError(type, $"The Keyword {type.lexeme} does not exist within the PROJECTILES block");

            }

        }
    }
}
