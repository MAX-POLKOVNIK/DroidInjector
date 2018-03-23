using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Polkovnik.DroidInjector.Fody.Loggers;

namespace Polkovnik.DroidInjector.Fody.Harvesters
{
    internal class ViewEventHarvester : Harvester
    {
        public ViewEventHarvester(ModuleDefinition moduleDefinition) : base(moduleDefinition)
        {
        }
        
        protected override bool NeedToHarvest(IMemberDefinition memberDefinition)
        {
            if (!(memberDefinition is MethodDefinition))
                return false;

            return memberDefinition.CustomAttributes.Any(x => x.AttributeType.FullName == Consts.InjectorAttributes.ViewEventAttributeTypeName);
        }

        public override IDictionary<TypeDefinition, IMemberDefinition[]> Harvest()
        {
            var result = base.Harvest();

            Logger.Info($"Found {result.SelectMany(x => x.Value).Count()} methods to subscribe in {result.Keys.Count} types");

            return result;
        }
    }
}
