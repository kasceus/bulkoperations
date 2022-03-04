namespace BulkOperations.Exceptions;
/// <summary>
/// used when a key is not found on the class used to upload data
/// </summary>
public class NoKeyFoundException : Exception
{
	/// <summary>
	/// used when a key is not found on the class used to upload data
	/// </summary>
	public NoKeyFoundException() : this("The supplied List type class did not have a key defined. " +
		"A key must be specified by using the BulkOptions class or by using the Key attribute in the class constructor.")
	{
	}
	public NoKeyFoundException(string message) : base(message)
	{
	}
	public NoKeyFoundException(string message, Exception inner) : base(message, inner)
	{
	}
}
