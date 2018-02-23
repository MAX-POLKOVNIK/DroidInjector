using System;
using System.Reflection;
using Android.Views;

namespace Polkovnik.DroidInjector.Internal
{
    internal class ViewClickMethodSubscriber : MethodSubscriber
    {
        public ViewClickMethodSubscriber(ViewEventAttribute viewEventHandlerAttribute, object eventOwner, object methodOwner, MethodInfo targetMethodInfo) 
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
            if (TargetMethodInfo.GetParameters().Length == 0)
            {
                ((View)EventOwner).Click += (sender, args) => TargetMethodInfo.Invoke(MethodOwner, new object[0]);
            }
            else
            {
                base.SubscribeToEvent();
            }
        }
    }
}