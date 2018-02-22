using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Android.Views;

namespace Polkovnik.DroidInjector
{
    internal sealed class Injector
    {
        #region - SINGLETON -

        private static Injector _instance;
        private static readonly object Lock = new object();

        private Injector()
        {
        }

        public static Injector Instance
        {
            get
            {
                lock (Lock)
                {
                    return _instance ?? (_instance = new Injector());
                }
            }
        }

        #endregion // SINGLETON

        private readonly Func<IMenu, int, IMenuItem> _menuItemRetriever = (menu, resourceId) => menu.FindItem(resourceId);
        private readonly Func<View, int, View> _viewRetriever = (view, resourceId) => view.FindViewById(resourceId);

        private readonly Action<FieldInfo, object, object> _fieldValueSetter = (member, owner, injectedObject) => member.SetValue(owner, injectedObject);
        private readonly Action<PropertyInfo, object, object> _propertyValueSetter = (member, owner, injectedObject) =>
        {
            if (member.SetMethod == null)
            {
                throw new InjectorException($"{member.Name} in class {member.DeclaringType} hasn't SET method. Properties with set methods allowed only");
            }

            member.SetValue(owner, injectedObject);
        };

        internal void InjectViews<T>(T injectableObject, View view, bool allowViewMissing = false)
        {
            InjectInternal<InjectViewAttribute, T, View>(injectableObject, view, _viewRetriever, allowViewMissing);
        }

        internal void InjectMenuItems<T>(T instance, IMenu menu, bool allowMenuItemsMissing = false)
        {
            InjectInternal<InjectMenuItemAttribute, T, IMenu>(instance, menu, _menuItemRetriever, allowMenuItemsMissing);
        }

        internal void BindViewActions<T>(T instance, View view, bool allowViewMissing)
        {
            var runtimeMethods = typeof(T).GetRuntimeMethods();
            
            foreach (var runtimeMethod in runtimeMethods)
            {
                var attributes = runtimeMethod.GetCustomAttributes(false);

                var bindViewActionAttribute = (ViewEventHandlerAttribute)attributes.SingleOrDefault(x => x is ViewEventHandlerAttribute);
                
                if (bindViewActionAttribute == null)
                    continue;

                var interactiveView = _viewRetriever.Invoke(view, bindViewActionAttribute.ResourceId);

                ValidateMissing(interactiveView, bindViewActionAttribute.ResourceId, runtimeMethod.Name, allowViewMissing);

                if (!bindViewActionAttribute.IsMethodSuitable(runtimeMethod))
                    throw new InjectorException("Not suitable method");

                bindViewActionAttribute.SubscribeToEvent(instance, interactiveView, runtimeMethod);
            }
        }

        static void AddEventHandler(EventInfo eventInfo, object item, Action action)
        {
            var parameters = eventInfo.EventHandlerType
                .GetMethod("Invoke")
                ?.GetParameters()
                .Select(parameter => Expression.Parameter(parameter.ParameterType))
                .ToArray();

            var handler = Expression.Lambda(
                    eventInfo.EventHandlerType,
                    Expression.Call(Expression.Constant(action), "Invoke", Type.EmptyTypes),
                    parameters
                )
                .Compile();

            eventInfo.AddEventHandler(item, handler);
        }
        static void AddEventHandler(EventInfo eventInfo, object item, Action<object, EventArgs> action)
        {
            var parameters = eventInfo.EventHandlerType
                .GetMethod("Invoke")
                ?.GetParameters()
                .Select(parameter => Expression.Parameter(parameter.ParameterType))
                .ToArray();

            var invoke = action.GetType().GetMethod("Invoke");

            var handler = Expression.Lambda(
                    eventInfo.EventHandlerType,
                    Expression.Call(Expression.Constant(action), invoke, parameters[0], parameters[1]),
                    parameters
                )
                .Compile();

            eventInfo.AddEventHandler(item, handler);
        }

        private void InjectInternal<TInjectAttribute, TOwner, TProvider>(TOwner instance, TProvider provider, 
            Func<TProvider, int, IDisposable> retriever, bool allowInjectMissing)
            where TInjectAttribute : IInjectAttribute
        {
            var runtimeFields = typeof(TOwner).GetRuntimeFields();

            foreach (var runtimeField in runtimeFields)
            {
                Inject<TInjectAttribute, FieldInfo, TProvider>(instance, runtimeField, provider, retriever, _fieldValueSetter, allowInjectMissing);
            }

            var runtimeProperties = instance.GetType().GetRuntimeProperties();

            foreach (var runtimeProperty in runtimeProperties)
            {
                Inject<TInjectAttribute, PropertyInfo, TProvider>(instance, runtimeProperty, provider, retriever, _propertyValueSetter, allowInjectMissing);
            }
        }
        
        private static void Inject<TInjectAttribute, TMember, TProvider>(object owner, TMember memberInfo,
            TProvider injectProvider, Func<TProvider, int, IDisposable> retriever, Action<TMember, object, object> setter, 
            bool allowInjectMissing) 
            where TMember : MemberInfo 
            where TInjectAttribute : IInjectAttribute
        {
            var attributes = memberInfo.GetCustomAttributes(false);

            var injectAttribute = attributes.SingleOrDefault(x => x is TInjectAttribute);

            if (injectAttribute == null)
                return;

            var resourceId = ((TInjectAttribute)injectAttribute).ResourceId;
            var canBeNull = ((TInjectAttribute)injectAttribute).CanBeNull;

            var injectedObject = retriever.Invoke(injectProvider, resourceId);
            
            ValidateMissing(injectedObject, resourceId, memberInfo.Name, allowInjectMissing || canBeNull);

            setter.Invoke(memberInfo, owner, injectedObject);
        }

        private static void ValidateMissing(IDisposable injectObject, int resourceId, string memberName, bool allowViewMissing)
        {
            if (injectObject != null || allowViewMissing)
                return;

            throw new InjectorException($"Can't find resource with ID = \"{resourceId}\" which injects to \"{memberName}\"");
        }

        private static string FindConstantName<TContainingType, TConstType>(TConstType value)
        {
            var containingType = typeof(TContainingType);

            var comparer = EqualityComparer<TConstType>.Default;

            return (from field in containingType.GetFields(BindingFlags.Static | BindingFlags.Public) where field.FieldType == typeof(TConstType) && comparer.Equals(value, (TConstType)field.GetValue(null)) select field.Name).FirstOrDefault();
        }
    }
}