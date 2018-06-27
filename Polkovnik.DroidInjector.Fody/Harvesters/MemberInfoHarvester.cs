using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace Polkovnik.DroidInjector.Fody.Harvesters
{
    internal class MemberInfoHarvester
    {
        private readonly ModuleDefinition _moduleDefinition;
        private readonly MemberInfoHarvestQuery[] _memberInfoHarvestQueries;

        public MemberInfoHarvester(ModuleDefinition moduleDefinition, params MemberInfoHarvestQuery[] memberInfoHarvestQueries)
        {
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
            _memberInfoHarvestQueries = memberInfoHarvestQueries;
        }

        public void Execute()
        {
            foreach (var type in _moduleDefinition.Types)
            {
                var members = new List<IMemberDefinition>();
                members.AddRange(type.Fields);
                members.AddRange(type.Properties);
                members.AddRange(type.Methods);

                foreach (var member in members)
                {
                    foreach (var memberInfoHarvestQuery in _memberInfoHarvestQueries)
                    {
                        if (memberInfoHarvestQuery.NeedToHarvest(member))
                            memberInfoHarvestQuery.Add(type, member);
                    }
                }
            }
        }
    }
}
