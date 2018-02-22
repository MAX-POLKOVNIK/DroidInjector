using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// Base inject attribute.
    /// </summary>
    public abstract class InjectAttribute : Attribute
    {
        internal int ResourceId { get; set; }
        internal bool CanBeNull { get; set; }
    }
}