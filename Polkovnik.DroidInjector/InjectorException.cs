using System;

namespace Polkovnik.DroidInjector
{
    /// <inheritdoc />
    /// <summary>
    /// Throws when something went wrong with injecting.
    /// </summary>
    public sealed class InjectorException : Exception
    {
        public InjectorException(string message) : base(message)
        {
        }
    }
}