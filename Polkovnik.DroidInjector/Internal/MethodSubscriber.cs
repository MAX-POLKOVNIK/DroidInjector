using System;
using System.Linq;
using System.Reflection;

namespace Polkovnik.DroidInjector.Internal
{
    internal class MethodSubscriber
    {
        public MethodSubscriber(ViewEventHandlerAttribute viewEventHandlerAttribute, object eventOwner, object methodOwner, MethodInfo targetMethodInfo)
        {
            MethodOwner = methodOwner ?? throw new ArgumentNullException(nameof(methodOwner));
            EventOwner = eventOwner ?? throw new ArgumentNullException(nameof(eventOwner));
            ViewEventHandlerAttribute = viewEventHandlerAttribute ?? throw new ArgumentNullException(nameof(viewEventHandlerAttribute));
            TargetMethodInfo = targetMethodInfo ?? throw new ArgumentNullException(nameof(targetMethodInfo));

            EventInfo = eventOwner.GetType().GetEvent(ViewEventHandlerAttribute.EventName);

            if (EventInfo == null)
                throw new InjectorException($"Can't find event \"{ViewEventHandlerAttribute.EventName}\" in \"{EventOwner.GetType().Name}\" class");

        }

        protected object MethodOwner { get; }
        protected object EventOwner { get; }
        protected ViewEventHandlerAttribute ViewEventHandlerAttribute { get; }
        protected MethodInfo TargetMethodInfo { get; }
        protected EventInfo EventInfo { get; }

        public virtual bool IsMethodSuitable
        {
            get
            {
                var methodParameters = TargetMethodInfo.GetParameters();
                var eventParameters = EventInfo.EventHandlerType.GetMethod(nameof(Action.Invoke))?.GetParameters() 
                    ?? throw new InjectorException("Can't find invoke method");
                
                if  (methodParameters.Length != eventParameters.Length)
                    return false;
                
                return !methodParameters.Where((t, i) => t.ParameterType != eventParameters[i].ParameterType).Any();
            }
        }

        public virtual void SubscribeToEvent()
        {
            Delegate handler;

            try
            {
                handler = Delegate.CreateDelegate(EventInfo.EventHandlerType, MethodOwner, TargetMethodInfo);
            }
            catch (ArgumentException)
            {
                throw new InjectorException($"Method {TargetMethodInfo.Name} not suitable for event {ViewEventHandlerAttribute.EventName}");
            }

            EventInfo.AddEventHandler(EventOwner, handler);
        }
    }
}