using System;
using System.Diagnostics.CodeAnalysis;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// MenuItem will be resolved at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
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

        /// <summary>
        /// MenuItem with <paramref name="resourceIdName"/> will be resolved at runtime.
        /// </summary>
        /// <param name="resourceIdName">Injected menuItem's id field name.</param>
        /// <param name="allowMissing">If true - injector will ignore missing menuItem.</param>
        public MenuItemAttribute(string resourceIdName, bool allowMissing)
        {
        }
    }
}