using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// View will be resolved at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ViewAttribute : InjectAttribute
    {
        /// <summary>
        /// View with <paramref name="resourceId"/> will be resolved at runtime.
        /// </summary>
        /// <param name="resourceId">Injected view's id.</param>
        /// <param name="allowMissing">If true - injector will ignore missing view.</param>
        public ViewAttribute(int resourceId = 0, bool allowMissing = false) : base(resourceId, allowMissing)
        {
        }

        public ViewAttribute(string resourceIdName, bool allowMissing = false) : base(resourceIdName, allowMissing)
        {
        }

        internal int ResourceId { get; set; }
        internal bool AllowMissing { get; set; }
        internal string ResourceIdName { get; set; }
    }
}