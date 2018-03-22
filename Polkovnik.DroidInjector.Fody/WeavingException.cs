using System;

namespace Polkovnik.DroidInjector.Fody
{
    public class WeavingException : Exception
    {
        public WeavingException(string message) : base(message)
        {
        }
    }
}
