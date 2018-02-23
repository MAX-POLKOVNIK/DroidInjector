using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// EXPERIMENTAL.
    /// Subscribes to view events at runtime. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ViewEventHandlerAttribute : InjectAttribute
    {
        /// <summary>
        /// Subscribes to view events at runtime. 
        /// </summary>
        /// <param name="resourceId">Id of view that has event to subscribe.</param>
        /// <param name="eventName">Event to subscribe.</param>
        /// <param name="canBeNull">If true - injector will ignore view missing.</param>
        public ViewEventHandlerAttribute(int resourceId, string eventName, bool canBeNull = false)
        {
            ResourceId = resourceId;
            EventName = eventName;
            CanBeNull = canBeNull;
        }
        
        internal string EventName { get; }
    }
}