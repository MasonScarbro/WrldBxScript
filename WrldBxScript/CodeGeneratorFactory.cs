using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    internal class CodeGeneratorFactory
    {
        private readonly Dictionary<string, ICodeGenerator> generators;
        private readonly Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> _repositories;
        private readonly Dictionary<string, object> _globals;
        public CodeGeneratorFactory(Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> repositories, Dictionary<string, object> globals)
        {
            _repositories = repositories;
            _globals = globals;
            generators = new Dictionary<string, ICodeGenerator>
            {
                { "EFFECTS", new EffectsCodeGenerator(_repositories) },
                { "TRAITS", new TraitsCodeGenerator(_repositories, _globals) },
                { "PROJECTILES", new ProjectilesCodeGenerator(_repositories) },
                { "STATUSES", new StatusesCodeGenerator(_repositories) },
                { "TERRAFORMING", new TerraformCodeGenerator(_repositories) },
                { "UNITS", new UnitsCodeGenerator(_repositories, _globals) }
            };
            
            
        }
        public ICodeGenerator GetGenerator(string type) =>
            generators.TryGetValue(type, out var generator)
                ? generator
                : throw new InvalidOperationException($"No generator found for type: {type}");
    }
}
