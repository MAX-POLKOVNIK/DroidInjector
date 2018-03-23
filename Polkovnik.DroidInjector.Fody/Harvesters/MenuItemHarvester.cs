using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Polkovnik.DroidInjector.Fody.Loggers;

namespace Polkovnik.DroidInjector.Fody.Harvesters
{
    internal class MenuItemHarvester : Harvester
    {
        public MenuItemHarvester(ModuleDefinition moduleDefinition) : base(moduleDefinition)
        {
        }

        protected override bool NeedToHarvest(IMemberDefinition memberDefinition)
        {
            switch (memberDefinition)
            {
                case FieldDefinition _:
                case PropertyDefinition _:
                    return memberDefinition.CustomAttributes.Any(x => x.AttributeType.FullName == Consts.InjectorAttributes.MenuItemAttributeTypeName);
                default:
                    return false;
            }
        }

        public override IDictionary<TypeDefinition, IMemberDefinition[]> Harvest()
        {
            var result = base.Harvest();

            Logger.Info($"Found {result.SelectMany(x => x.Value).Count()} members to inject menu items in {result.Keys.Count} types");

            return result;
        }
    }
}
