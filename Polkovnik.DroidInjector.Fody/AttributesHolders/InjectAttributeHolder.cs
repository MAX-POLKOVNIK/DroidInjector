using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;

namespace Polkovnik.DroidInjector.Fody.AttributesHolders
{
    internal abstract class InjectAttributeHolder
    {
        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        protected InjectAttributeHolder(CustomAttribute injectAttribute)
        {
            var fullName = injectAttribute.AttributeType.FullName;

            if (injectAttribute.AttributeType.FullName != RequiredAttributeName)
                throw new WeavingException($"Wrong attribute: Required {RequiredAttributeName}, Passed {fullName}");
            
            switch (injectAttribute.ConstructorArguments[0].Value)
            {
                case int resourceId:
                    ResourceId = resourceId;
                    break;
                case string resourceIdName:
                    ResourceIdName = resourceIdName;
                    break;
                default:
                    throw new WeavingException($"Wrong attribute {injectAttribute.AttributeType.Name}. Can't find resource id parameter");
            }

            AllowMissing = (bool)injectAttribute.ConstructorArguments[1].Value;
        }

        protected abstract string RequiredAttributeName { get; }

        internal int ResourceId { get; }
        internal bool AllowMissing { get; }
        internal string ResourceIdName { get; }
    }
}
