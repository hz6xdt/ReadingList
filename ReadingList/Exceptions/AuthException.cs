namespace ReadingList.Exceptions
{
    public class AuthException(string message) : Exception(message)
    {
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

        public AuthException(string message, IEnumerable<string> errors) : this(message)
        {
            Errors = errors;
        }
    }
}
