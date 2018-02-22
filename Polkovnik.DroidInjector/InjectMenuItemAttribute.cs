using System;

namespace Polkovnik.DroidInjector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InjectMenuItemAttribute : InjectAttribute
    {
        public InjectMenuItemAttribute(int resourceId, bool canBeNull = false)
        {
            ResourceId = resourceId;
            CanBeNull = canBeNull;
        }
    }
}