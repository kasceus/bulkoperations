using System.Runtime.Serialization;

namespace BulkOperations.Exceptions;
/// <summary>
/// This exception is for when the sql runs the bulk operation and the number of records passed do not match the number returned from the database
/// </summary>
[Serializable]
public class RecordCountMismatchException : Exception
{
	private const string message = "The number of records passed are not the same as the amount actually updated/inserted/deleted in the Database. Transaction was roleld back.";
	/// <summary>
	/// This exception is for when the sql runs the bulk operation and the number of records passed do not match the number returned from the database
	/// </summary>
	public RecordCountMismatchException() : this(message)
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
	/// <summary>
	/// This exception is for when the sql runs the bulk operation and the number of records passed do not match the number returned from the database
	/// </summary>
	/// <param name="info">serialization info</param>
	/// <param name="context">streaming context</param>
	/// <remarks>This is only added to shut the compiler up about some serialization issue</remarks>
	protected RecordCountMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
	{

	}
}
