using Android.App;
using Android.Views;

namespace Polkovnik.DroidInjector
{
    public static class InjectExtensions
    {
        /// <summary>
        /// Injects views in marked fields and properties. 
        /// </summary>
        /// <param name="activity">Activity that containts fields and properties to inject.</param>
        /// <param name="allowViewMissing">If true - injector will ignore view missing.</param>
        public static void InjectViews<TActivity>(this TActivity activity, bool allowViewMissing = false) where TActivity : Activity
        {
            var view = activity.FindViewById<ViewGroup>(Android.Resource.Id.Content);
            InjectViews(activity, view, allowViewMissing);
        }

        /// <summary>
        /// Injects views in marked fields and properties. 
        /// </summary>
        /// <param name="injectableObject">Class that containts fields and properties to inject.</param>
        /// <param name="view">Container.</param>
        /// <param name="allowViewMissing">If true - injector will ignore view missing.</param>
        public static void InjectViews<T>(this T injectableObject, View view, bool allowViewMissing = false)
        {
            Injector.Instance.InjectViews(injectableObject, view, allowViewMissing);
        }

        /// <summary>
        /// Injects menuItems in marked fields and properties. 
        /// </summary>
        /// <param name="instance">Class that containts fields and properties to inject.</param>
        /// <param name="menu">Container.</param>
        /// <param name="allowMenuItemsMissing">If true - injector will ignore menuItems missing.</param>
        public static void InjectMenuItems<T>(this T instance, IMenu menu, bool allowMenuItemsMissing = false)
        {
            Injector.Instance.InjectMenuItems(instance, menu, allowMenuItemsMissing);
        }

        /// <summary>
        /// Subscribes marked methods to events.
        /// </summary>
        /// <param name="activity">Activity that containts methods to subscribe.</param>
        /// <param name="allowViewMissing">If true - injector will ignore views missing.</param>
        public static void BindViewActions<TActivity>(this TActivity activity, bool allowViewMissing = false) where TActivity : Activity
        {
            var view = activity.FindViewById<ViewGroup>(Android.Resource.Id.Content);
            BindViewActions(activity, view, allowViewMissing);
        }

        /// <summary>
        /// Subscribes marked methods to events.
        /// </summary>
        /// <param name="instance">Class that containts methods to subscribe.</param>
        /// <param name="view">Container.</param>
        /// <param name="allowViewMissing">If true - injector will ignore views missing.</param>
        public static void BindViewActions<T>(this T instance, View view, bool allowViewMissing = false)
        {
            Injector.Instance.BindViewActions(instance, view, allowViewMissing);
        }
    }
}