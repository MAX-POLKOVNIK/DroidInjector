using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Polkovnik.DroidInjector.Fody.Harvesters
{
    internal abstract class Harvester
    {
        private readonly ModuleDefinition _moduleDefinition;

        protected Harvester(ModuleDefinition moduleDefinition)
        {
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
        }

        protected abstract bool NeedToHarvest(IMemberDefinition memberDefinition);

        public virtual IDictionary<TypeDefinition, IMemberDefinition[]> Harvest()
        {
            var membersDictionary = new Dictionary<TypeDefinition, List<IMemberDefinition>>();

            foreach (var type in _moduleDefinition.Types)
            {
                var members = new List<IMemberDefinition>();
                members.AddRange(type.Fields);
                members.AddRange(type.Properties);
                members.AddRange(type.Methods);

                foreach (var member in members)
                {
                    if (!NeedToHarvest(member))
                        continue;

                    if (membersDictionary.TryGetValue(type, out var list))
                    {
                        list.Add(member);
                    }
                    else
                    {
                        membersDictionary.Add(type, new List<IMemberDefinition> { member });
                    }
                }
            }

            return membersDictionary.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
        }
    }
}
