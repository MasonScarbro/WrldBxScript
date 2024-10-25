using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public class WrldBxObjectRepository<T> where T : IWrldBxObject
    {
        private readonly Dictionary<string, T> objects = new Dictionary<string, T>();
        private readonly Func<string, T> objectFactory;

        public WrldBxObjectRepository(Func<string, T> objectFactory)
        {
            this.objectFactory = objectFactory;
        }

        public void UpdateObject(string id, Token type, object value)
        {
            if (!objects.ContainsKey(id))
            {
                objects.Add(id, objectFactory(id));
            }
            else
            {
                objects[id].UpdateStats(type, value);
            }
        }

        public T GetObject(string id)
        {
            return objects.TryGetValue(id, out T obj) ? obj : default;
        }

        public IEnumerable<T> GetAll => objects.Values;

        public bool Exists(string id) { return objects.ContainsKey(id); }
    }
}
