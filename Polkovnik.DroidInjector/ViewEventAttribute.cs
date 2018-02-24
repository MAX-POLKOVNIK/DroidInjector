﻿using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// EXPERIMENTAL.
    /// Subscribes to view events at runtime. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ViewEventAttribute : InjectAttribute
    {
        /// <summary>
        /// Subscribes to view events at runtime. 
        /// </summary>
        /// <param name="resourceId">Id of view that has event to subscribe.</param>
        /// <param name="eventName">Event to subscribe.</param>
        /// <param name="allowMissing">If true - injector will ignore view missing.</param>
        public ViewEventAttribute(int resourceId, string eventName, bool allowMissing = false) : base(resourceId, allowMissing)
        {
            EventName = eventName;
        }

        public ViewEventAttribute(int resourceId, Type viewType, string eventName, bool allowMissing = false) : this(resourceId,
            eventName, allowMissing)
        {
            ViewType = viewType;
        }

        internal Type ViewType { get; }
        internal string EventName { get; }
    }
}