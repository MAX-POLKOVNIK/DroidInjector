using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Polkovnik.DroidInjector.Fody.Loggers;

namespace Polkovnik.DroidInjector.Fody.Harvesters
{
    internal class ViewHarvester : Harvester
    {
        public ViewHarvester(ModuleDefinition moduleDefinition) : base(moduleDefinition)
        {
        }

        protected override bool NeedToHarvest(IMemberDefinition memberDefinition)
        {
            switch (memberDefinition)
            {
                case FieldDefinition _:
                case PropertyDefinition _:
                    return memberDefinition.CustomAttributes.Any(x => x.AttributeType.FullName == Consts.InjectorAttributes.ViewAttributeTypeName);
                default:
                    return false;
            }
        }

        public override IDictionary<TypeDefinition, IMemberDefinition[]> Harvest()
        {
            var result = base.Harvest();

            Logger.Info($"Found {result.SelectMany(x => x.Value).Count()} members to inject views in {result.Keys.Count} types");

            return result;
        }
    }
}
