using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// EXPERIMENTAL.
    /// Subscribes to view events at runtime. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ViewEventAttribute : Attribute
    {
        public ViewEventAttribute(int resourceId, Type viewType, string eventName, bool allowMissing = false)
        {
        }
    }
}