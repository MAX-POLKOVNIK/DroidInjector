using System;
using System.Diagnostics.CodeAnalysis;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// EXPERIMENTAL.
    /// Subscribes to view events at runtime. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    [Obsolete("Use direct event subscribtion instead of this attribute. Automatic event subscription has been removed", true)]
    public class ViewEventAttribute : Attribute
    {
        public ViewEventAttribute(int resourceId, Type viewType, string eventName, bool allowMissing = false)
        {
        }
    }
}