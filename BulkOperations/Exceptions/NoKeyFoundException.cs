using System.Runtime.Serialization;

namespace BulkOperations.Exceptions;
/// <summary>
/// used when a key is not found on the class passed to the bulkOperations
/// </summary>
[Serializable]
public class NoKeyFoundException : Exception
{

	private const string constMessage = "The supplied List type class did not have a key defined. " +
		"A key must be specified by using the BulkOptions class or by using the Key attribute in the class constructor.";
	/// <summary>
	/// used when a key is not found on the class passed to the bulkOperations
	/// </summary>
	public NoKeyFoundException() : this(constMessage)
	{
	}
	/// <summary>
	/// used when a key is not found on the class passed to the bulkOperations
	/// </summary>
	/// <param name="message">custom error message</param>
	public NoKeyFoundException(string message) : base(message)
	{
	}
	/// <summary>
	/// used when a key is not found on the class passed to the bulkOperations
	/// </summary>
	public NoKeyFoundException(string message, Exception inner) : base(message, inner)
	{
	}
	/// <summary>
	///used when a key is not found on the class passed to the bulkOperations
	/// </summary>
	/// <param name="info">serialization info</param>
	/// <param name="context">streaming context</param>
	/// <remarks>This is only added to shut the compiler up about some serialization issue</remarks>
	protected NoKeyFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
	{

	}
}
