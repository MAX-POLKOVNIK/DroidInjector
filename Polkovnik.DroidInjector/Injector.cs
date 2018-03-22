using System;
using Android.Views;

namespace Polkovnik.DroidInjector
{
    /// <summary>
    /// Injector. 
    /// </summary>
    public sealed class Injector
    {
        /// <summary>
        /// Call this method only in types derived from Activity.
        /// Starts resolving views.
        /// </summary>
        public static void InjectViews()
        {
            throw new InjectorException($"Method {nameof(InjectViews)} must be removed at runtime. Check if Fody is working.");
        }
        
        /// <summary>
        /// Starts resolving views.
        /// </summary>
        /// <param name="view">View provider.</param>
        public static void InjectViews(View view)
        {
            throw new InjectorException($"Method {nameof(InjectViews)} must be removed at runtime. Check if Fody is working.");
        }

        public static void BindViewEvents()
        {
            throw new InjectorException($"Method {nameof(BindViewEvents)} must be removed at runtime. Check if Fody is working.");
        }

        public static void BindViewEvents(View view)
        {
            throw new InjectorException($"Method {nameof(BindViewEvents)} must be removed at runtime. Check if Fody is working.");
        }

        public static void InjectMenuItems(IMenu menu)
        {
            throw new InjectorException($"Method {nameof(InjectMenuItems)} must be removed at runtime. Check if Fody is working.");
        }
    }
}