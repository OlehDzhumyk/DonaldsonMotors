namespace DonaldsonMotors.API.Exceptions
{
    /// <summary>
    /// Thrown when a record was changed by someone else between reading and saving it
    /// (for example two people using the last units of a part at once). Maps to 409 Conflict.
    /// </summary>
    public class ConcurrencyConflictException : InvalidOperationException
    {
        public ConcurrencyConflictException(Exception innerException)
            : base("Someone else changed this record at the same time. Please reload and try again.", innerException) { }
    }
}
