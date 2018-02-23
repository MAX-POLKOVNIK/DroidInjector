using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.Views;
using Polkovnik.DroidInjector.Internal;

namespace Polkovnik.DroidInjector
{
    /// <summary>
    /// Injector. 
    /// </summary>
    public sealed class Injector
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
        
        internal Type ResourceClassType { get; set; }
        internal Type ResourceIdClassType { get; set; }

        /// <summary>
        /// Registers Resource and Resource.Id classes.
        /// Required for using experimental features.
        /// </summary>
        /// <typeparam name="TResource">Resource</typeparam>
        /// <typeparam name="TResourceId">Id</typeparam>
        public void RegisterResourceClass<TResource, TResourceId>()
        {
            ResourceClassType = typeof(TResource);
            ResourceIdClassType = typeof(TResourceId);
        }

        internal void InjectViews<T>(T injectableObject, View view, bool allowViewMissing = false)
        {
            InjectInternal<ViewAttribute, T, View>(injectableObject, view, RetrieveView, allowViewMissing);
        }

        internal void InjectMenuItems<T>(T instance, IMenu menu, bool allowMenuItemsMissing = false)
        {
            InjectInternal<MenuItemAttribute, T, IMenu>(instance, menu, RetrieveMenuItem, allowMenuItemsMissing);
        }

        internal void BindViewActions<T>(T instance, View view, bool allowViewMissing)
        {
            var runtimeMethods = typeof(T).GetRuntimeMethods();
            
            foreach (var runtimeMethod in runtimeMethods)
            {
                Inject<ViewEventAttribute, MethodInfo, View>(instance, runtimeMethod, view, RetrieveView, SubscribeEvent, allowViewMissing);
            }
        }
        
        private void InjectInternal<TInjectAttribute, TOwner, TProvider>(TOwner instance, TProvider provider,
            Func<TProvider, InjectAttribute, MemberInfo, IDisposable> retriever, bool allowInjectMissing)
            where TInjectAttribute : InjectAttribute
        {
            var runtimeFields = typeof(TOwner).GetRuntimeFields();

            foreach (var runtimeField in runtimeFields)
            {
                Inject<TInjectAttribute, FieldInfo, TProvider>(instance, runtimeField, provider, retriever, SetField, allowInjectMissing);
            }

            var runtimeProperties = instance.GetType().GetRuntimeProperties();

            foreach (var runtimeProperty in runtimeProperties)
            {
                Inject<TInjectAttribute, PropertyInfo, TProvider>(instance, runtimeProperty, provider, retriever, SetProperty, allowInjectMissing);
            }
        }
        
        private void Inject<TInjectAttribute, TMember, TProvider>(object owner, TMember memberInfo,
            TProvider injectProvider, Func<TProvider, InjectAttribute, MemberInfo, IDisposable> retriever, Action<TInjectAttribute, TMember, object, object> handler, 
            bool allowInjectMissing) 
            where TMember : MemberInfo 
            where TInjectAttribute : InjectAttribute
        {
            var attributes = memberInfo.GetCustomAttributes(false);

            var injectAttribute = (TInjectAttribute)attributes.SingleOrDefault(x => x is TInjectAttribute);

            if (injectAttribute == null)
                return;

            var resourceId = injectAttribute.ResourceId;
            var canBeNull = injectAttribute.CanBeNull;

            var injectedObject = retriever.Invoke(injectProvider, injectAttribute, memberInfo);
            
            ValidateMissing(injectedObject, resourceId, memberInfo.Name, allowInjectMissing || canBeNull);

            handler.Invoke(injectAttribute, memberInfo, owner, injectedObject);
        }

        private void ValidateMissing(IDisposable injectObject, int resourceId, string memberName, bool allowViewMissing)
        {
            if (injectObject != null || allowViewMissing)
                return;

            var id = ResourceIdClassType == null
                ? resourceId.ToString()
                : FindConstantName(ResourceIdClassType, resourceId);

            throw new InjectorException($"Can't find resource with ID = \"{id}\" which injects to \"{memberName}\"");
        }

        private void SetField(InjectAttribute injectAttribute, FieldInfo info, object owner, object injectedObject)
        {
            info.SetValue(owner, injectedObject);
        }

        private void SetProperty(InjectAttribute injectAttribute, PropertyInfo info, object owner, object injectedObject)
        {
            if (info.SetMethod == null)
            {
                throw new InjectorException($"{info.Name} in class {info.DeclaringType} hasn't SET method. Properties with set methods allowed only");
            }

            info.SetValue(owner, injectedObject);
        }

        private void SubscribeEvent(ViewEventAttribute viewEventHandlerAttribute, MethodInfo info, object owner, object injectedObject)
        {
            var subscriber = MethodSubscriberFactory.Create(viewEventHandlerAttribute, injectedObject, owner, info);
            
            if (!subscriber.IsMethodSuitable)
                throw new InjectorException("Not suitable method");

            subscriber.SubscribeToEvent();
        }

        private IMenuItem RetrieveMenuItem(IMenu menu, InjectAttribute attribute, MemberInfo memberInfo)
        {
            return menu.FindItem(attribute.ResourceId);
        }

        private View RetrieveView(View container, InjectAttribute attribute, MemberInfo memberInfo)
        {
            int resourceId;

            if (attribute.ResourceId != 0)
            {
                resourceId = attribute.ResourceId;
            }
            else
            {
                if (ResourceIdClassType == null)
                    throw new InjectorException($"You can't use parameterless attribute {nameof(ViewAttribute)} without calling {nameof(RegisterResourceClass)}");

                var fieldName = memberInfo.Name.Trim('_');

                var field = ResourceIdClassType.GetField(fieldName);

                if (field == null)
                    throw new InjectorException($"Can't find field in Resource.Id with name {fieldName}");

                resourceId = (int)field.GetValue(null);

            }
            
            return container.FindViewById(resourceId);
        }

        private static string FindConstantName<TConstType>(Type containingType, TConstType value)
        {
            var comparer = EqualityComparer<TConstType>.Default;

            return (from field in containingType.GetFields(BindingFlags.Static | BindingFlags.Public) where field.FieldType == typeof(TConstType) && comparer.Equals(value, (TConstType)field.GetValue(null)) select field.Name).FirstOrDefault();
        }
    }
}