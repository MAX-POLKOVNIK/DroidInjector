using System;

namespace Polkovnik.DroidInjector.Fody
{
    /// <inheritdoc />
    /// <summary>
    /// If throwed will be displayed as build error.
    /// </summary>
    public class WeavingException : Exception
    {
        public WeavingException(string message) : base(message)
        {
        }
    }
}
