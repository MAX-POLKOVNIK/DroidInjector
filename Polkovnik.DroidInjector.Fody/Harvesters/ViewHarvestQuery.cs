using System.Linq;
using Mono.Cecil;

namespace Polkovnik.DroidInjector.Fody.Harvesters
{
    internal class ViewHarvestQuery : MemberInfoHarvestQuery
    {
        public override bool NeedToHarvest(IMemberDefinition memberDefinition)
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
    }
}
