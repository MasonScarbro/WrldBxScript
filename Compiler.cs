using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    class Compiler
    {

        public void Compile(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    //Execute(statement);
                }
            }
            catch (CompilerError error)
            {
                WrldBxScript.CompilerErrorToCons(error);
            }
        }



    }
}
