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
        /// EXPERIMENTAL.
        /// View with name of field or property will be resolved at runtime.
        /// You must to register Resource class with <see cref="Injector"/>.
        /// </summary>
        public ViewAttribute() : base(0, false)
        {
        }

        /// <summary>
        /// View with <paramref name="resourceId"/> will be resolved at runtime.
        /// </summary>
        /// <param name="resourceId">Injected view's id.</param>
        /// <param name="allowMissing">If true - injector will ignore missing view.</param>
        public ViewAttribute(int resourceId, bool allowMissing = false) : base(resourceId, allowMissing)
        {
        }
    }
}