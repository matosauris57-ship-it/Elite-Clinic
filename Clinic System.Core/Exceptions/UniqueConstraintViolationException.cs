namespace Clinic_System.Core.Exceptions
{
    public class UniqueConstraintViolationException : Exception
    {
        public UniqueConstraintViolationException() { }

        public UniqueConstraintViolationException(string message) : base(message)
        {
        }

        public UniqueConstraintViolationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
