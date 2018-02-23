using System.Reflection;

namespace Polkovnik.DroidInjector.Internal
{
    internal class MethodSubscriberFactory
    {
        public static MethodSubscriber Create(ViewEventAttribute attribute, object eventOwner, object methodOwner, MethodInfo targetMethodInfo)
        {
            switch (attribute)
            {
                case ViewClickEventAttribute _:
                    return new ViewClickMethodSubscriber(attribute, eventOwner, methodOwner, targetMethodInfo);
                default: 
                    return new MethodSubscriber(attribute, eventOwner, methodOwner, targetMethodInfo);
            }
        }
    }
}