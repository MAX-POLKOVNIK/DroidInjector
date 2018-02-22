using Android.App;
using Android.Views;

namespace Polkovnik.DroidInjector
{
    public static class InjectExtensions
    {
        public static void InjectViews<TActivity>(this TActivity activity, bool allowViewMissing = false) where TActivity : Activity
        {
            var view = activity.FindViewById<ViewGroup>(Android.Resource.Id.Content);
            InjectViews(activity, view, allowViewMissing);
        }

        public static void InjectViews<T>(this T injectableObject, View view, bool allowViewMissing = false)
        {
            Injector.Instance.InjectViews(injectableObject, view, allowViewMissing);
        }

        public static void InjectMenuItems<T>(this T instance, IMenu menu, bool allowMenuItemsMissing = false)
        {
            Injector.Instance.InjectMenuItems(instance, menu, allowMenuItemsMissing);
        }

        public static void BindViewActions<TActivity>(this TActivity activity, bool allowViewMissing = false) where TActivity : Activity
        {
            var view = activity.FindViewById<ViewGroup>(Android.Resource.Id.Content);
            BindViewActions(activity, view, allowViewMissing);
        }

        public static void BindViewActions<T>(this T instance, View view, bool allowViewMissing = false)
        {
            Injector.Instance.BindViewActions(instance, view, allowViewMissing);
        }
    }
}