namespace NotionFinance.Exceptions;

public class NotionDatabaseNotFoundException : Exception
{
    public NotionDatabaseNotFoundException() {}
    public NotionDatabaseNotFoundException(string message): base(message) {}
}