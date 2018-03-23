using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// View will be resolved at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ViewAttribute : Attribute
    {
        /// <summary>
        /// View with <paramref name="resourceId"/> will be resolved at runtime.
        /// </summary>
        /// <param name="resourceId">Injected view's id.</param>
        /// <param name="allowMissing">If true - injector will ignore missing view.</param>
        public ViewAttribute(int resourceId = 0, bool allowMissing = false)
        {
        }

        public ViewAttribute(string resourceIdName, bool allowMissing = false)
        {
        }
    }
}