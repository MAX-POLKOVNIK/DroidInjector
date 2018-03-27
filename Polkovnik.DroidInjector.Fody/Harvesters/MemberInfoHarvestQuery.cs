using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Polkovnik.DroidInjector.Fody.Harvesters
{
    internal abstract class MemberInfoHarvestQuery
    {
        private readonly Dictionary<TypeDefinition, List<IMemberDefinition>> _membersDictionary = new Dictionary<TypeDefinition, List<IMemberDefinition>>();

        public virtual IDictionary<TypeDefinition, IMemberDefinition[]> QueryResult => _membersDictionary.ToDictionary(x => x.Key, x => x.Value.ToArray());

        public abstract bool NeedToHarvest(IMemberDefinition memberDefinition);

        public virtual void Add(TypeDefinition typeDefinition, IMemberDefinition memberDefinition)
        {
            if (_membersDictionary.TryGetValue(typeDefinition, out var list))
            {
                list.Add(memberDefinition);
            }
            else
            {
                _membersDictionary.Add(typeDefinition, new List<IMemberDefinition> { memberDefinition });
            }
        }
    }
}
