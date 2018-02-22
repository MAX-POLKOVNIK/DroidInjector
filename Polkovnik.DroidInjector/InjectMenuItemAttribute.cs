using System;

namespace Polkovnik.DroidInjector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InjectMenuItemAttribute : Attribute, IInjectAttribute
    {
        public InjectMenuItemAttribute(int resourceId, bool canBeNull = false)
        {
            ResourceId = resourceId;
            CanBeNull = canBeNull;
        }

        public int ResourceId { get; }
        public bool CanBeNull { get; }
    }
}