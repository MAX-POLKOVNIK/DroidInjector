using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// Base inject attribute.
    /// </summary>
    public abstract class InjectAttribute : Attribute
    {
        protected InjectAttribute(int resourceId, bool allowMissing)
        {
            ResourceId = resourceId;
            AllowMissing = allowMissing;
        }

        internal int ResourceId { get; set; }
        internal bool AllowMissing { get; set; }
    }
}