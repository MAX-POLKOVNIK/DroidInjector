using System;
using System.Reflection;
using Android.Views;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// EXPERIMENTAL.
    /// Subscribes to view click event at runtime. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ViewClickEventHandlerAttribute : ViewEventHandlerAttribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Subscribes to view click event at runtime. 
        /// </summary>
        /// <param name="resourceId">Id of view that has event to subscribe.</param>
        /// <param name="canBeNull">If true - injector will ignore view missing.</param>
        public ViewClickEventHandlerAttribute(int resourceId, bool canBeNull = false) : base(resourceId, null, canBeNull)
        {
        }

        internal override bool IsMethodSuitable(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();

            return parameters.Length == 0
                   || parameters.Length == 2 
                        && typeof(object).IsAssignableFrom(parameters[0].ParameterType) 
                        && typeof(EventArgs).IsAssignableFrom(parameters[1].ParameterType);
        }

        internal override void SubscribeToEvent(object handlerOwner, View viewWithEvent, MethodInfo methodInfo)
        {
            viewWithEvent.Click += (sender, args) => methodInfo.Invoke(handlerOwner, methodInfo.GetParameters().Length == 0 ? new object[0] : new [] {sender, args}); 
        }
    }
}