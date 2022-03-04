namespace BulkOperations.Exceptions;

public class ItemPassedAsNullException : Exception
{
	public ItemPassedAsNullException() : this("Item passed to string builder as null.")
	{

	}
	public ItemPassedAsNullException(string message) : base(message)
	{

	}
}
