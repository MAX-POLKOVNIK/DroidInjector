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
    public class ViewEventAttribute : Attribute
    {
        public ViewEventAttribute(int resourceId, Type viewType, string eventName, bool allowMissing = false)
        {
        }
    }
}