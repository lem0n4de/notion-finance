namespace NotionFinance.Exceptions;

public class NotionPageNotFoundException : Exception
{
    public NotionPageNotFoundException()
    {
    }

    public NotionPageNotFoundException(string message) : base(message)
    {
    }
}