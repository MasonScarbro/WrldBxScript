using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    class Compiler
    {
        private int count = 0;
        private string src;
        public void Compile(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    var stmt = Execute(statement);
                }
            }
            catch (CompilerError error)
            {
                WrldBxScript.CompilerErrorToCons(error);
            }
        }

        private Stmt Execute(Stmt stmt)
        {
            if (stmt is Stmt.Var stmtv)
            {
                if (stmtv.type.lexeme.ToUpper() == "HEALTH")
                {
                    
                }
            }
            if (stmt is Stmt.Starter stmtst)
            {
                foreach (Stmt.Block block in stmtst.body)
                {
                    src = "class " + ToParaCase(stmtst.type.lexeme) + "\n" + "{" + "\n\tpublic static void init() \n{";
                    
                    Execute(block);
                }
                // reset src and count at the end of executing a starter since it descends
                count = 0;
                src = "";
            }
            if (stmt is Stmt.Block stmtb)
            {
                foreach (Stmt stat in stmtb.statements)
                {
                    
                    Execute(stat);
                }
            }
            if (stmt is Stmt.Expression stmte)
            {
                return stmte;
            }
            return null;
        }

        
        private string ToParaCase(string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
        }
    }
}
