using System.Reflection;

namespace Polkovnik.DroidInjector.Internal
{
    internal class MethodSubscriberFactory
    {
        public static MethodSubscriber Create(ViewEventHandlerAttribute attribute, object eventOwner, object methodOwner, MethodInfo targetMethodInfo)
        {
            switch (attribute)
            {
                case ViewClickEventHandlerAttribute _:
                    return new ParameterlessMethodSubscriber(attribute, eventOwner, methodOwner, targetMethodInfo);
                default: 
                    return new MethodSubscriber(attribute, eventOwner, methodOwner, targetMethodInfo);
            }
        }
    }
}