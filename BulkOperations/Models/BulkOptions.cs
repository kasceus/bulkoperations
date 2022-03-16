namespace BulkOperations.Models;
/// <summary>
/// Configurable options for bulk operations
/// </summary>
public class BulkOptions
{
	/// <summary>
	/// How many records to insert/update/delete at a time
	/// </summary>
	public int BatchSize { get; set; } = 1000;
	///<summary>
	/// Keys used in the table. If not provided, then an attempt will be made to get the keys using the Key attribute.
	/// </summary>
	public string[] KeyNames { get; set; } = Array.Empty<string>();
	/// <summary>
	/// Insert the primary key value into the database.  <para>This will be ignored if the key has a databasegenerated option =true attribute set</para>
	/// </summary>
	public bool InsertKey { get; set; } = false;
	/// <summary>
	/// This is used to get the model name vs the databse name of the column in the class
	/// </summary>
	internal List<ColumnKeyInfo> ColumnValues { get; set; } = new();
	/// <summary>
	/// Columns that contain data that was changed. Use this option to speed up the process when all data has not been updated.
	/// </summary>
	public string[] ColumnsToUpdate { get; set; } = Array.Empty<string>();
	/// <summary>
	/// Only update the data for the specified columns. Default is true.
	/// </summary>
	public bool OnlyUpdateSpecifiedColumns { get; set; } = true;
	/// <summary>
	/// Automatically truncate strings for fields with values longer than allowed in the database
	/// </summary>
	public bool TruncateStrings { get; set; } = false;

}
