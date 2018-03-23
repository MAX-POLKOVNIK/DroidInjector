using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// Base inject attribute.
    /// </summary>
    public abstract class InjectAttribute : Attribute
    {
        protected InjectAttribute(string resourceIdName, bool allowMissing)
        {
        }

        protected InjectAttribute(int resourceId, bool allowMissing)
        {
        }
    }
}