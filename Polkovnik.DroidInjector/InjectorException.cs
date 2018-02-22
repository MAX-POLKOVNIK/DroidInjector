using System;

namespace Polkovnik.DroidInjector
{
    public sealed class InjectorException : Exception
    {
        public InjectorException(string message) : base(message)
        {

        }
    }
}