namespace BulkOperations.Models;
/// <summary>
/// Configurable options for bulk operations
/// </summary>
public class BulkOptions
{
	/// <summary>
	/// use transactions when commiting records to database
	/// </summary>
	public bool UseTransactions { get; set; } = true;
	/// <summary>
	/// How many records to insert/update/delete at a time
	/// </summary>
	public int BatchSize { get; set; } = 1000;
	///<summary>
	/// Keys used in the table. If not provided, then an attempt will be made to get the keys using the Key attribute.
	/// </summary>
	public string[] KeyNames { get; set; } = Array.Empty<string>();
	/// <summary>
	/// This is used to get the model name vs the databse name of the column in the class
	/// </summary>
	internal List<ColumnKeyInfo> ColumnValues { get; set; } = new();
	/// <summary>
	/// Columns that contain data that was changed
	/// </summary>
	public string[] ColumnsToUpdate { get; set; } = Array.Empty<string>();
	/// <summary>
	/// Only update the data for the specified columns.
	/// </summary>
	public bool OnlyUpdateSpecifiedColumns { get; set; } = true;
	/// <summary>
	/// Automatically truncate strings for fields with values longer than allowed in the database
	/// </summary>
	public bool TruncateStrings { get; set; } = false;

}
