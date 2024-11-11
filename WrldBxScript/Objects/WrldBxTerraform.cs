
using System;

namespace WrldBxScript
{
    public class WrldBxTerraform : IWrldBxObject
    {
        public string id { get; set; }
        public bool addBurned { get; set; }
        public bool flash { get; set; }
        public bool applyForce { get; set; }
        public double force_power { get; set; }
        public bool explode_tile { get; set; }
        public double? explode_strength { get; set; }
        public bool damageBuildings { get; set; }
        public bool setFire { get; set; }
        public bool shake { get; set; }
       
        public double damage { get; set; }

        public WrldBxTerraform(string id)
        {
            this.id = id;
            this.addBurned = false;
            this.flash = true;
            this.explode_tile = true;
            this.setFire = false;
            this.shake = false;
            this.damageBuildings = true;
            this.explode_strength = 1;
            this.force_power = 1;
            this.damage = 10;



        }

        public void UpdateStats(Token type, object value)
        {
            try
            {
                switch (type.type)
                {

                    case TokenType.ADDBURNED:
                        addBurned = bool.Parse(value.ToString());
                        break;
                    case TokenType.APPLYFORCE:
                        applyForce = bool.Parse(value.ToString());
                        break;
                    case TokenType.EXPLODE_TILE:
                        explode_tile = bool.Parse(value.ToString());
                        break;
                    case TokenType.EXPLODE_STRENGTH:
                        explode_strength = double.Parse(value.ToString());
                        break;
                    case TokenType.DAMAGEBUILDINGS:
                        damageBuildings = bool.Parse(value.ToString());
                        break;
                    case TokenType.SETFIRE:
                        setFire = bool.Parse(value.ToString());
                        break;
                    case TokenType.SHAKE:
                        shake = bool.Parse(value.ToString());
                        break;
                    case TokenType.DAMAGE:
                        damage = float.Parse(value.ToString());
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
