using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

using BulkOperations.Exceptions;
using BulkOperations.Models;

using static BulkOperations.Methods.StaticMethods;

namespace BulkOperations.Methods;

internal static class SqlStringMaker
{
	internal static string MakeString<T>(IList<T> data, OperationType operationType, BulkOptions options)
	{
		if (data is null || data.Count == 0)
		{
			throw new ArgumentNullException(nameof(data));
		}
		StringBuilder sb = new();
		sb.Append(operationType.ToString());
		switch (operationType)
		{
			case OperationType.Delete:
				sb.Append(" from");
				break;
			case OperationType.Insert:
				sb.Append(" into");
				break;
			case OperationType.Update:
			default:
				break;
		}
#pragma warning disable CS8602 // Dereference of a possibly null reference. This should never be null if it gets here
		switch (data.First().GetType().GetCustomAttribute(typeof(TableAttribute)))
#pragma warning restore CS8602 // Dereference of a possibly null reference.
		{
			case TableAttribute tableAttribute:
				sb.Append(' ').Append(tableAttribute.Name);
				break;
			default:
				sb.Append(' ').Append(data.GetType().Name);
				break;
		}
		sb.Append(' ');//space separator between command initializer and the rest of the sequence
		if (!options.KeyNames.Any())
		{
			options.KeyNames = GetKeys(data).Result;
		}
		if (options.ColumnsToUpdate == null || options.ColumnsToUpdate.Length == 0)
		{
			options.ColumnValues = GetNonKeyColumnInfo(data, options.KeyNames);
		}
		else
		{
			options.ColumnValues = GetSpecifiedColumnInfo(data, options.ColumnsToUpdate);
		}
		if (operationType == OperationType.Update)
		{
			bool first = true;

			foreach (ColumnKeyInfo column in options.ColumnValues)
			{
				string paramString = GetUpdateParamString(data, column, options.KeyNames).Result;
				if (!first)
				{
					paramString = paramString.Replace("set", ",");
				}
				sb.Append(paramString);
				first = false;
			}
		}
		else if (operationType == OperationType.Delete)
		{
			sb.Append(GetDeleteString(data, options.KeyNames));
		}

		return sb.ToString();
	}
	#region Update string builders
	internal static async Task<string> GetUpdateParamString<T>(IList<T> data, ColumnKeyInfo columnName, string[] keyNames)
	{
		StringBuilder sb = new();
		sb.Append($"set {columnName.ColumnName} = (Case ");
		Task t = Task.Run(() =>
		{
			//now loop through all the records and set the case values bassed on the columnName
			foreach (T? item in data)
			{
				string value = GetColumnValue(item, columnName.ModelPropName);
				if (item is not null)
				{
					sb.Append(MakeWhen(keyNames, item));
					sb.Append($" then '{value}' ");
				}
			}
		});
		await t;
		sb.Append("end)");
		return sb.ToString();
	}
	/// <summary>
	/// this is used in the update param string method only
	/// </summary>
	/// <typeparam name="T">Generic type param</typeparam>
	/// <param name="keyNames">Names of the keys</param>
	/// <param name="item">item reference</param>
	/// <returns>string for the param's when clause</returns>
	/// <exception cref="ItemPassedAsNullException"></exception>
	private static string MakeWhen<T>(string[] keyNames, T item)
	{
		if (item == null)
		{
			throw new ItemPassedAsNullException();
		}
		StringBuilder sb = new();
		sb.Append("when ");
		for (int i = 0; i < keyNames.Length; i++)
		{
			Tuple<string, string>? nameAndValue = GetTableNameAndValue(item, keyNames[i]);
			sb.Append($"{nameAndValue.Item1} = '{nameAndValue.Item2}'");
			if (i < keyNames.Length - 1)
			{
				sb.Append(" and ");
			}
		}
		return sb.ToString();
	}


	#endregion
	#region Insert String Builders


	#endregion

	#region delete string builders

	private static string GetDeleteString<T>(IList<T> data, string[] keyNames)
	{
		//string needs to be : $"where ({keyname} = '{keyvalue}' and {otherKeyName} ='{otherKeyValue}') or ({keyname} = '{keyvalue}' and {otherKeyName} ='{otherKeyValue}') ...(repeat as necessary)
		StringBuilder sb = new();
		sb.Append(" where ");
		Parallel.ForEach(data, item =>
		{
			StringBuilder stringBuilder = new();
			stringBuilder.Append('(');
			for (int i = 0; i < keyNames.Length; i++)
			{
				Tuple<string, string>? itemData = GetTableNameAndValue(item, keyNames[i]);
				lock (sb)
				{
					stringBuilder.Append($"{itemData.Item1} = '{itemData.Item2}' ");
				}
				if (i < keyNames.Length - 1)
				{
					stringBuilder.Append("and ");
				}
			}
			stringBuilder.Append(") or ");
			lock (sb)
			{
				sb.Append(stringBuilder.ToString());
			}
		});
		string? retString = sb.ToString().Trim();
		string? endor = retString.Substring(retString.Length - 2);
		if (endor.Contains("or"))
		{
			retString = retString.Substring(0, retString.Length - 2);
		}
		return retString.Trim();
	}
	#endregion

	public static string GetColumnValue<T>(T item, string columnName)
	{
		if (item is null)
		{
			throw new ItemPassedAsNullException();
		}
		return item.GetType().GetProperty(columnName).GetValue(item).ToString();
	}

	/// <summary>
	/// Gets the name of the item used in the table if it has a column.name attribute- otherwise it will use the name specified in the model item passed in.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="item"></param>
	/// <param name="keyName"></param>
	/// <returns></returns>
	/// <exception cref="ItemPassedAsNullException"></exception>
	private static Tuple<string, string> GetTableNameAndValue<T>(T item, string keyName)
	{
		if (item is null)
		{
			throw new ItemPassedAsNullException();
		}
		PropertyInfo? key = item.GetType().GetProperty(keyName);
		string tableKeyName;
		if (key.GetCustomAttribute(typeof(ColumnAttribute)) is ColumnAttribute column && !string.IsNullOrWhiteSpace(column.Name))
		{ //the table key name might be different than the class name get that here
			tableKeyName = column.Name;
		}
		else
		{
			tableKeyName = keyName;
		}
		string keyValue = key.GetValue(item).ToString();

		return Tuple.Create(tableKeyName, keyValue);
	}

}
