using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public interface ICodeGenerator
    {
        void GenerateCode(StringBuilder src, string modname);
        void AddBlockId(StringBuilder src, object name);
        void AddReqCodeToBlock(StringBuilder src, object name, string appendage = null);

        
    }
}
