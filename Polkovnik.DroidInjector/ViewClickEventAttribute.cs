using System;
using Android.Views;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// EXPERIMENTAL.
    /// Subscribes to view click event at runtime. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ViewClickEventAttribute : ViewEventAttribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Subscribes to view click event at runtime. 
        /// </summary>
        /// <param name="resourceId">Id of view that has event to subscribe.</param>
        /// <param name="allowMissing">If true - injector will ignore view missing.</param>
        public ViewClickEventAttribute(int resourceId, bool allowMissing = false) : base(resourceId, nameof(View.Click), allowMissing)
        {
        }
    }
}