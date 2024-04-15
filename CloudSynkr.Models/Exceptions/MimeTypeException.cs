namespace CloudSynkr.Models.Exceptions;

public class MimeTypeException : Exception
{
    public MimeTypeException()
    {
    }

    public MimeTypeException(string message)
        : base(message)
    {
    }

    public MimeTypeException(string message, Exception inner)
        : base(message, inner)
    {
    }
}