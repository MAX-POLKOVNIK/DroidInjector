using System.Linq;
using Mono.Cecil;

namespace Polkovnik.DroidInjector.Fody.Harvesters
{
    internal class ViewEventHarvestQuery : MemberInfoHarvestQuery
    {
        public override bool NeedToHarvest(IMemberDefinition memberDefinition)
        {
            if (!(memberDefinition is MethodDefinition))
                return false;

            return memberDefinition.CustomAttributes.Any(x => x.AttributeType.FullName == Consts.InjectorAttributes.ViewEventAttributeTypeName);
        }
    }
}
