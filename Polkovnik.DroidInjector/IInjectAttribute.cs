using System;

namespace Polkovnik.DroidInjector
{
    public abstract class InjectAttribute : Attribute
    {
        internal int ResourceId { get; set; }
        internal bool CanBeNull { get; set; }
    }
}