using System;

namespace DonaldsonMotors.API.Exceptions
{
    public class ResourceInUseException : InvalidOperationException
    {
        public ResourceInUseException(string message) : base(message) { }
    }
}