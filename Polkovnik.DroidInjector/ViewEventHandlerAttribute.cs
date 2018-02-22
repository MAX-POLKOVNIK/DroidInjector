using System;
using System.Reflection;
using Android.Views;

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

        internal virtual string GetNotSutableMethodErrorDescription(string methodName) => $"Method {methodName} not suitable for event {EventName}";

        internal virtual void SubscribeToEvent(object handlerOwner, View viewWithEvent, MethodInfo methodInfo)
        {
            var eventInfo = viewWithEvent.GetType().GetEvent(EventName);

            if (eventInfo == null)
                throw new InjectorException($"Can't find event \"{EventName}\" in \"{viewWithEvent.GetType().Name}\" class");

            Delegate handler;

            try
            {
                handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, handlerOwner, methodInfo);
            }
            catch (ArgumentException)
            {
                throw new InjectorException(GetNotSutableMethodErrorDescription(methodInfo.Name));
            }
            
            eventInfo.AddEventHandler(viewWithEvent, handler);
        }

        internal virtual bool IsMethodSuitable(MethodInfo methodInfo)
        {
            return true;
        }
    }
}