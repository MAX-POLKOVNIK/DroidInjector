using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Polkovnik.DroidInjector.Internal
{
    internal class ParameterlessMethodSubscriber : MethodSubscriber
    {
        public ParameterlessMethodSubscriber(ViewEventHandlerAttribute viewEventHandlerAttribute, object eventOwner, object methodOwner, MethodInfo targetMethodInfo) 
            : base(viewEventHandlerAttribute, eventOwner, methodOwner, targetMethodInfo)
        {
        }

        public override bool IsMethodSuitable
        {
            get
            {
                var parameters = TargetMethodInfo.GetParameters();

                return parameters.Length == 0
                       || parameters.Length == 2
                       && typeof(object).IsAssignableFrom(parameters[0].ParameterType)
                       && typeof(EventArgs).IsAssignableFrom(parameters[1].ParameterType);
            }
        }

        public override void SubscribeToEvent()
        {
            var @delegate = TargetMethodInfo.GetParameters().Length == 0
                ? Create(EventInfo, (Action)Delegate.CreateDelegate(typeof(Action), MethodOwner, TargetMethodInfo))
                : Delegate.CreateDelegate(EventInfo.EventHandlerType, MethodOwner, TargetMethodInfo);

            EventInfo.AddEventHandler(EventOwner, @delegate);
        }

        private Delegate Create(EventInfo eventInfo, Action action)
        {
            var handlerType = eventInfo.EventHandlerType;
            var eventParams = handlerType.GetMethod(nameof(Action.Invoke))?.GetParameters() ?? throw new InjectorException("Can't find invoke method");
            
            var parameters = eventParams.Select(p => Expression.Parameter(p.ParameterType, "x"));
            var method = action.GetType().GetMethod(nameof(Action.Invoke)) ?? throw new InjectorException("Can't find invoke method");
            var body = Expression.Call(Expression.Constant(action), method);
            var lambda = Expression.Lambda(body, parameters.ToArray());
            return Delegate.CreateDelegate(handlerType, lambda.Compile(), nameof(Action.Invoke), false);
        }
    }
}