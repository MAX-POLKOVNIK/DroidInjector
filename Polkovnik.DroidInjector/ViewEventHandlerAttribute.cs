using System;
using System.Diagnostics;
using System.Reflection;
using Android.Views;

namespace Polkovnik.DroidInjector
{
    public class ViewEventHandlerAttribute : Attribute, IInjectAttribute
    {
        public ViewEventHandlerAttribute(int resourceId, string eventName, bool canBeNull = false)
        {
            ResourceId = resourceId;
            EventName = eventName;
            CanBeNull = canBeNull;
        }
        
        public string EventName { get; }
        public int ResourceId { get; }
        public bool CanBeNull { get; }

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