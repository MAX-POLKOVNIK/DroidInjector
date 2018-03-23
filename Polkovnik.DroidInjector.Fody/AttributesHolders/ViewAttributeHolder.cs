using Mono.Cecil;

namespace Polkovnik.DroidInjector.Fody.AttributesHolders
{
    internal class ViewAttributeHolder : InjectAttributeHolder
    {
        public ViewAttributeHolder(CustomAttribute customAttribute) : base(customAttribute)
        {
            
        }

        protected override string RequiredAttributeName => Consts.InjectorAttributes.ViewAttributeTypeName;
    }
}
