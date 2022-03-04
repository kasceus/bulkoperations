namespace BulkOperations.Exceptions;
/// <summary>
/// This exception is for when the sql runs the bulk operation and the number of records passed do not match the number returned from the database
/// </summary>
public class RecordCountMismatchException : Exception
{
	/// <summary>
	/// This exception is for when the sql runs the bulk operation and the number of records passed do not match the number returned from the database
	/// </summary>
	public RecordCountMismatchException() : this("The number of records passed are not the same as the amount actually updated/inserted/deleted in the Database. Transaction was roleld back.")
	{

	}
	/// <summary>
	/// This exception is for when the sql runs the bulk operation and the number of records passed do not match the number returned from the database
	/// </summary>
	public RecordCountMismatchException(string message) : base(message)
	{

	}
	/// <summary>
	/// This exception is for when the sql runs the bulk operation and the number of records passed do not match the number returned from the database
	/// </summary>
	public RecordCountMismatchException(string message, Exception inner) : base(message, inner)
	{

	}
}
