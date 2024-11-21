using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer.Collaboration;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace WrldBxScript.Objects
{
    internal class WrldBxKingdom : IWrldBxObject
    {
        
        public string id { get; set; }
        public bool isMob { get; set; }
        public List<object> friendly;
        public List<object> enemy;
        public WrldBxKingdom(string id)
        {
            this.id = id;
            isMob = true;
        }
        public void UpdateStats(Token type, object value)
        {
            try
            {
                switch (type.type)
                {
                    case TokenType.ID:

                        id = value.ToString();
                        break;

                    case TokenType.FRIENDLY:
                        friendly = new List<object>();
                        if (value is List<object> list)
                        {
                            friendly.AddRange(list.Select(item => item));
                        }
                        else
                        {
                            friendly.Add(value);
                        }
                        break;

                    case TokenType.ENEMY:
                        enemy = new List<object>();
                        if (value is List<object> liste)
                        {
                            enemy.AddRange(liste.Select(item => item));
                        }
                        else
                        {
                            enemy.Add(value);
                        }
                        break;
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
