using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.Views;

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
        
        private readonly Action<FieldInfo, object, object> _fieldValueSetter = (member, owner, injectedObject) => member.SetValue(owner, injectedObject);
        private readonly Action<PropertyInfo, object, object> _propertyValueSetter = (member, owner, injectedObject) =>
        {
            if (member.SetMethod == null)
            {
                throw new InjectorException($"{member.Name} in class {member.DeclaringType} hasn't SET method. Properties with set methods allowed only");
            }

            member.SetValue(owner, injectedObject);
        };

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
            InjectInternal<InjectViewAttribute, T, View>(injectableObject, view, RetrieveView, allowViewMissing);
        }

        internal void InjectMenuItems<T>(T instance, IMenu menu, bool allowMenuItemsMissing = false)
        {
            InjectInternal<InjectMenuItemAttribute, T, IMenu>(instance, menu, RetrieveMenuItem, allowMenuItemsMissing);
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

                var interactiveView = RetrieveView(view, bindViewActionAttribute, runtimeMethod);

                ValidateMissing(interactiveView, bindViewActionAttribute.ResourceId, runtimeMethod.Name, allowViewMissing);

                if (!bindViewActionAttribute.IsMethodSuitable(runtimeMethod))
                    throw new InjectorException("Not suitable method");

                bindViewActionAttribute.SubscribeToEvent(instance, interactiveView, runtimeMethod);
            }
        }

        private void InjectInternal<TInjectAttribute, TOwner, TProvider>(TOwner instance, TProvider provider,
            Func<TProvider, InjectAttribute, MemberInfo, IDisposable> retriever, bool allowInjectMissing)
            where TInjectAttribute : InjectAttribute
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
        
        private void Inject<TInjectAttribute, TMember, TProvider>(object owner, TMember memberInfo,
            TProvider injectProvider, Func<TProvider, InjectAttribute, MemberInfo, IDisposable> retriever, Action<TMember, object, object> setter, 
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

            setter.Invoke(memberInfo, owner, injectedObject);
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
                    throw new InjectorException($"You can't use parameterless attribute {nameof(InjectViewAttribute)} without calling {nameof(RegisterResourceClass)}");

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