namespace BASE.Models
{

    public class ValidException : Exception
    {
        public ValidException()
        { }

        public ValidException(string message)
            : base(message)
        { }

        public ValidException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
