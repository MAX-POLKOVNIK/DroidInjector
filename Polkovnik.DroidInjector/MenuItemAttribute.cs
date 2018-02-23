using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// MenuItem will be resolved at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class MenuItemAttribute : InjectAttribute
    {
        /// <summary>
        /// MenuItem with <paramref name="resourceId"/> will be resolved at runtime.
        /// </summary>
        /// <param name="resourceId">Injected menuItem's id.</param>
        /// <param name="canBeNull">If true - injector will ignore missing menuItem.</param>
        public MenuItemAttribute(int resourceId, bool canBeNull = false) : base(resourceId, canBeNull)
        {
        }
    }
}