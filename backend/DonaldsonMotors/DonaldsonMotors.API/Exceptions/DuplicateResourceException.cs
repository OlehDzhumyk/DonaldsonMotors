using System;

namespace DonaldsonMotors.API.Exceptions
{
    public class DuplicateResourceException : InvalidOperationException 
    {
        public DuplicateResourceException(string message) : base(message) { }
    }
}