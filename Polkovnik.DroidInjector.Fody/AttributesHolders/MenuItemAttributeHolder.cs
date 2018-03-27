using Mono.Cecil;

namespace Polkovnik.DroidInjector.Fody.AttributesHolders
{
    internal class MenuItemAttributeHolder : InjectAttributeHolder
    {
        public MenuItemAttributeHolder(CustomAttribute customAttribute) : base(customAttribute)
        {
        }

        protected override string RequiredAttributeName => Consts.InjectorAttributes.MenuItemAttributeTypeName;
    }
}
