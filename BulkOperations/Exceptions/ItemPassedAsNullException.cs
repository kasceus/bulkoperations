using System.Runtime.Serialization;

namespace BulkOperations.Exceptions;

/// <summary>
/// Item was pased to a method from another method as null
/// </summary>
[Serializable]
public class ItemPassedAsNullException : Exception
{
	/// <summary>
	/// Item was pased to a method from another method as null
	/// </summary>
	public ItemPassedAsNullException() : this("Item passed to string builder as null.")
	{

	}
	/// <summary>
	/// Item was pased to a method from another method as null
	/// </summary>
	public ItemPassedAsNullException(string message) : base(message)
	{

	}
	/// <summary>
	/// Item was pased to a method from another method as null
	/// </summary>
	/// <param name="info">serialization info</param>
	/// <param name="context">streaming context</param>
	/// <remarks>This is only added to shut the compiler up about some serialization issue</remarks>
	protected ItemPassedAsNullException(SerializationInfo info, StreamingContext context) : base(info, context)
	{

	}
}
