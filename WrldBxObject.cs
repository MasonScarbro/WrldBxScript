using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public interface IWrldBxObject
    {
        string id { get; set; }

        void UpdateStats(Token type, object value);
    }
}
