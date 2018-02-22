using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// View will be resolved at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InjectViewAttribute : InjectAttribute
    {
        /// <summary>
        /// EXPERIMENTAL.
        /// View with name of field or property will be resolved at runtime.
        /// You must to register Resource class with <see cref="Injector"/>.
        /// </summary>
        public InjectViewAttribute()
        {
        }

        /// <summary>
        /// View with <paramref name="resourceId"/> will be resolved at runtime.
        /// </summary>
        /// <param name="resourceId">Injected view's id.</param>
        /// <param name="canBeNull">If true - injector will ignore missing view.</param>
        public InjectViewAttribute(int resourceId, bool canBeNull = false)
        {
            ResourceId = resourceId;
            CanBeNull = canBeNull;
        }
    }
}