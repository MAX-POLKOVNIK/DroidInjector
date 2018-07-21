namespace Polkovnik.DroidInjector.Fody
{
    internal class Consts
    {
        public const string AndroidRuntimeResourceDesignerAttributeName = "Android.Runtime.ResourceDesignerAttribute";

        internal class InjectorAttributes
        {
            public const string ViewAttributeTypeName = "Polkovnik.DroidInjector.ViewAttribute";
            public const string MenuItemAttributeTypeName = "Polkovnik.DroidInjector.MenuItemAttribute";
        }

        internal class GeneratedMethodNames
        {
            public const string InjectViewsGeneratedMethodName = "Polkovnik_DroidInjector_InjectViews";
            public const string InjectMenuItemsGeneratedMethodName = "Polkovnik_DroidInjector_InjectMenuItems";
        }
    }
}
