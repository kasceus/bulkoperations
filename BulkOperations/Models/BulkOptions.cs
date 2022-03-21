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
	/// <summary>
	/// This is used to get the model name vs the databse name of the column in the class
	/// </summary>
	internal List<ColumnKeyInfo> ColumnValues { get; set; } = new();
	/// <summary>
	/// Columns that contain data that was changed. Use this option to speed up the process when all data has not been updated.
	/// </summary>
	public string[] ColumnsToUpdate { get; set; } = Array.Empty<string>();
	///<summary>
	/// Keys used in the table. If not provided, then an attempt will be made to get the keys using the Key attribute.
	/// </summary>
	public string[] KeyNames { get; set; } = Array.Empty<string>();
	/// <summary>
	/// Specify any columns to ignore when inserting records.  
	/// <para>This is used when comparing the records to insert vs the records in the database. 
	/// An example use case would be if there is an indexed field and a datetime field where the datetime field is used to track changes. The datetime field
	/// should be ignored in that case since it will always be different, and could cause duplicate key problems when inserting the new records.
	/// </para>
	/// </summary>
	public string[] ColumnsToIgnore { get; set; }= Array.Empty<string>();
	/// <summary>
	/// If the model has inedxes that should be used for insertion specify them here.
	/// <para>Indexes are also used to compare insertion records for if inserted records are not already in the table.</para>
	/// </summary>
	public string[] IndexValues { get; set; }= Array.Empty<string>();
	/// <summary>
	/// Insert the primary key value into the database.  <para>This will be ignored if the key has a databasegenerated option =true attribute set</para>
	/// </summary>
	public bool InsertKey { get; set; } = false;	
	/// <summary>
	/// Only update the data for the specified columns. Default is true.
	/// </summary>
	public bool OnlyUpdateSpecifiedColumns { get; set; } = true;

}
