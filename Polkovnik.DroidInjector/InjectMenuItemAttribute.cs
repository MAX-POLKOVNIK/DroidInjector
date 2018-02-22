using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// MenuItem will be resolved at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InjectMenuItemAttribute : InjectAttribute
    {
        /// <summary>
        /// MenuItem with <paramref name="resourceId"/> will be resolved at runtime.
        /// </summary>
        /// <param name="resourceId">Injected menuItem's id.</param>
        /// <param name="canBeNull">If true - injector will ignore missing menuItem.</param>
        public InjectMenuItemAttribute(int resourceId, bool canBeNull = false)
        {
            ResourceId = resourceId;
            CanBeNull = canBeNull;
        }
    }
}