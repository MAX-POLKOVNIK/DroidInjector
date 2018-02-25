using Mono.Cecil;

namespace Polkovnik.DroidInjector.Fody
{
    internal class MethodSubscriptionInfo
    {
        public MethodSubscriptionInfo(MethodReference addHandlerMethod, MethodReference handlerCtor, MethodReference targetMethod, TypeReference ownerType)
        {
            AddHandlerMethod = addHandlerMethod;
            HandlerCtor = handlerCtor;
            TargetMethod = targetMethod;
            OwnerType = ownerType;
        }

        public MethodReference AddHandlerMethod { get; }
        public MethodReference HandlerCtor { get; }
        public MethodReference TargetMethod { get; }
        public TypeReference OwnerType { get; }
    }
}
