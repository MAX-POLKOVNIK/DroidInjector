using System;

namespace Polkovnik.DroidInjector.Fody
{
    public class FodyInjectorException : Exception
    {
        public FodyInjectorException(string message) : base(message)
        {
        }
    }
}
