using System;
using System.Reflection;
using Android.Views;

namespace Polkovnik.DroidInjector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InjectViewAttribute : InjectAttribute
    {
        public InjectViewAttribute()
        {
        }

        public InjectViewAttribute(int resourceId, bool canBeNull = false)
        {
            ResourceId = resourceId;
            CanBeNull = canBeNull;
        }
    }
}