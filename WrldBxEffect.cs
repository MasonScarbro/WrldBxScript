using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public class WrldBxEffect
    {
        private string id;
        private bool spawnsFromActor;

        public WrldBxEffect(string id)
        {
            this.id = id;
            this.spawnsFromActor = false;
        }

        public void UpdateStats(List<string> strings, object value)
        {
            foreach (string str in strings)
            {
                if (str.Equals("id")) id = value.ToString();
                if (str.Equals("spawnsFromActor")) spawnsFromActor = (bool)value;
            }
        }
    }
}
