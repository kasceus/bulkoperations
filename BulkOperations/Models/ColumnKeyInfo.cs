namespace BulkOperations.Models;
/// <summary>
/// This class is used to hold the column names of the model item vs the model database column name
/// </summary>
internal class ColumnKeyInfo
{
	/// <summary>
	/// Name of the property in the model
	/// </summary>
	internal string ModelPropName { get; set; } = "";
	/// <summary>
	/// The column Attribute name of the item used in the actual database
	/// </summary>
	internal string ColumnName { get; set; } = "";
}
