using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// Base inject attribute.
    /// </summary>
    public abstract class InjectAttribute : Attribute
    {
        protected InjectAttribute(int resourceId, bool canBeNull)
        {
            ResourceId = resourceId;
            CanBeNull = canBeNull;
        }

        internal int ResourceId { get; set; }
        internal bool CanBeNull { get; set; }
    }
}