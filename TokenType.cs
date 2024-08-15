using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public enum TokenType
    {

        //Single Char Tokens
        LEFT_BRACE, RIGHT_BRACE, COMMA, MINUS, PLUS, RIGHT_BRACKET, LEFT_BRACKET,

        //One Or Two Char Tokens (Maybe) Basically Just the equal
        COLON, 


        //Literals 
        IDENTIFIER, STRING, NUMBER, TRUE, FALSE, NIL,

        //Major Keywords
        MODNAME, TRAITS, EFFECTS, STATUSES, ITEMS,

        //Minor KeyWords Traits
        DAMAGE, HEALTH, ATTACK_SPEED, CRIT_CHANCE, RANGE, LOCALIZATION, ID, 
        DODGE, ACCURACY, SCALE, INTELIGENCE, WARFARE, STEWARDSHIP, PATH, TIMEBETWEENFRAMES,
        LIMIT, DRAW_LIGHT, DRAW_LIGHT_SIZE, SPAWNFROMACTOR, SPAWNONTARGET,

        //EOF
        EOF

        


    }
}
