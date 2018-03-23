using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// MenuItem will be resolved at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class MenuItemAttribute : Attribute
    {
        /// <summary>
        /// MenuItem with <paramref name="resourceId"/> will be resolved at runtime.
        /// </summary>
        /// <param name="resourceId">Injected menuItem's id.</param>
        /// <param name="allowMissing">If true - injector will ignore missing menuItem.</param>
        public MenuItemAttribute(int resourceId, bool allowMissing = false)
        {
        }

        public MenuItemAttribute(string resourceIdName, bool allowMissing)
        {
        }
    }
}