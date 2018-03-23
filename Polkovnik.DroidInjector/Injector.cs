using Android.Views;

namespace Polkovnik.DroidInjector
{
    /// <summary>
    /// Injector. 
    /// DO NOT PASS METHODS OF THIS CLASS AS DELEGATES!
    /// </summary>
    public sealed class Injector
    {
        /// <summary>
        /// Call this method only in types derived from Activity.
        /// Starts resolving views.
        /// This method will be replaced with generated method in activity.
        /// </summary>
        public static void InjectViews()
        {
            throw new InjectorException($"Method {nameof(InjectViews)} must be removed at runtime. Check if Fody is working.");
        }

        /// <summary>
        /// Starts resolving views.
        /// This method will be replaced with generated method in activity.
        /// </summary>
        /// <param name="view">View.</param>
        public static void InjectViews(View view)
        {
            throw new InjectorException($"Method {nameof(InjectViews)} must be removed at runtime. Check if Fody is working.");
        }

        /// <summary>
        /// Call this method only in types derived from Activity.
        /// Starts binding view events.
        /// This method will be replaced with generated method in activity.
        /// </summary>
        public static void BindViewEvents()
        {
            throw new InjectorException($"Method {nameof(BindViewEvents)} must be removed at runtime. Check if Fody is working.");
        }

        /// <summary>
        /// Starts binding view events.
        /// This method will be replaced with generated method in activity.
        /// </summary>
        /// <param name="view">View.</param>
        public static void BindViewEvents(View view)
        {
            throw new InjectorException($"Method {nameof(BindViewEvents)} must be removed at runtime. Check if Fody is working.");
        }

        /// <summary>
        /// Starts resolving menu items.
        /// This method will be replaced with generated method in activity.
        /// </summary>
        /// <param name="menu">Menu.</param>
        public static void InjectMenuItems(IMenu menu)
        {
            throw new InjectorException($"Method {nameof(InjectMenuItems)} must be removed at runtime. Check if Fody is working.");
        }
    }
}