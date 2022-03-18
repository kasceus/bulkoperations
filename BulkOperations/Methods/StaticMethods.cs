using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

using BulkOperations.Exceptions;
using BulkOperations.Models;

namespace BulkOperations.Methods;

internal static class StaticMethods
{
    /// <summary>
    ///Gets the keys for the given class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    internal static async Task<string[]> GetKeys<T>(IList<T> data)
    {
        PropertyInfo[]? props = data.FirstOrDefault()?.GetType().GetProperties();
        if (props == null)
        {
            throw new InvalidOperationException();
        }

        Task<string[]>? t = Task.Run(() =>
        {
            List<string>? keys = new();
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetCustomAttribute(typeof(KeyAttribute)) is KeyAttribute key)
                {
                    keys.Add(prop.Name.ToString());
                }
            }
            if (!keys.Any())
            {
                throw new KeyNotFoundException();
            }
            return keys.ToArray();
        });
        await t;
        string[]? keys = t.Result;
        return keys;
    }
    internal static async Task<string[]> GetIndexes<T>(IList<T> data, BulkOptions options   )
    {
        PropertyInfo[]? props = data.FirstOrDefault()?.GetType().GetProperties();
        if (props == null)
        {
            throw new InvalidOperationException();
        }

        Task<string[]>? t = Task.Run(() =>
        {
            List<string>? indeces = new();
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetCustomAttribute(typeof(IndexAttribute)) is IndexAttribute)
                {
                    if (prop.GetCustomAttribute(typeof(ColumnAttribute)) is ColumnAttribute column)
                    {                       
                        indeces.Add(column.Name.ToString());
                    }
                    else
                    {
                        indeces.Add(prop.Name.ToString());
                    }
                }
                else if (prop.GetCustomAttribute(typeof(KeyAttribute)) is KeyAttribute && !string.IsNullOrWhiteSpace(prop.GetValue(data.FirstOrDefault())?.ToString() ?? ""))
                { // fail over to the key value if it exists, but only if the key value is not null for the first record

                    if (prop.GetCustomAttribute(typeof(ColumnAttribute)) is ColumnAttribute column)
                    {
                        indeces.Add(column.Name.ToString());
                    }
                    else
                    {
                        indeces.Add(prop.Name.ToString());
                    }
                }
            }
            if (!indeces.Any())
            {
                throw new KeyNotFoundException();
            }
            if (!options.InsertKey)
            {
                return indeces.Where(a=>!options.KeyNames.Contains(a)).ToArray();

            }
            return indeces.ToArray();
        });
        await t;
        string[]? keys = t.Result;
        return keys;
    }
    /// <summary>
    /// This gets all the properties for a class except the specified keys.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="keys"></param>
    /// <param name="indexes"></param>
    /// <returns></returns>
    /// <exception cref="ItemPassedAsNullException"></exception>
    internal static List<ColumnKeyInfo> GetNonKeyColumnInfo<T>(IList<T> data, string[] keys, string[] indexes)
    {
        if (!data.Any() || data is null)
        {
            throw new ItemPassedAsNullException();
        }
        T? item = data.FirstOrDefault();
        if (item is null)
        {
            throw new ItemPassedAsNullException();
        }
        IEnumerable<PropertyInfo> columns = item.GetType().GetProperties().AsEnumerable();
        List<ColumnKeyInfo> cols = new();
        foreach (PropertyInfo? property in columns)
        {
            if (property is null)
            {
                throw new ItemPassedAsNullException("Could not find the property in the provided item");
            }
            bool isEnumerable = typeof(IList).IsAssignableFrom(property.PropertyType) 
                || typeof(ICollection).IsAssignableFrom(property.PropertyType) 
                || property.PropertyType.Name.ToLower().Contains("collection");//for some reason sometimes icollections come through as icollection`1 and don't work with the above isassignablefrom test
            if (property.GetMethod is null || property.GetMethod.IsVirtual || isEnumerable)
            { // ignore virtual types and ienumerable types
                continue;
            }
            if (property.GetCustomAttribute(typeof(DatabaseGeneratedAttribute)) is DatabaseGeneratedAttribute)
            {
                //don't return computed columns
                continue;
            }
            if (property.SetMethod == null || !property.SetMethod.IsPublic || (property.GetCustomAttribute(typeof(ReadOnlyAttribute)) is ReadOnlyAttribute attr && attr.IsReadOnly))
            {
                continue;//don't add readonly attributes
            }
            if (property.GetCustomAttribute(typeof(NotMappedAttribute)) is NotMappedAttribute)
            {
                continue;//item not mapped to a database column, don't add it
            }
            ColumnKeyInfo columnInfo = new()
            {
                ModelPropName = property.Name,
                ColumnName = property.Name
            };
            if (property.GetCustomAttribute(typeof(ColumnAttribute)) is ColumnAttribute columnAttribute && !string.IsNullOrWhiteSpace(columnAttribute.Name))
            {
                columnInfo.ColumnName = columnAttribute.Name;
            }
            cols.Add(columnInfo);
        }
        string[] forExclusion = keys.ToArray(); // exclude keys by default
        forExclusion = forExclusion.Except(indexes).ToArray();//unless that key is also an index
        return cols.Where(a => !forExclusion.ToList().Contains(a.ModelPropName)).ToList();
    }
    /// <summary>
    /// This gets the column info for the specified columns.
    /// <para> this is useful when a property has the column attribute with a different name that what is used in the class</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="columnArray"></param>
    /// <returns></returns>
    /// <exception cref="ItemPassedAsNullException"></exception>
    internal static List<ColumnKeyInfo> GetSpecifiedColumnInfo<T>(IList<T> data, string[] columnArray)
    {
        if (!data.Any() || data is null)
        {
            throw new ItemPassedAsNullException();
        }
        T? item = data.FirstOrDefault();
        if (item is null)
        {
            throw new ItemPassedAsNullException();
        }
        IEnumerable<PropertyInfo>? props = item.GetType().GetProperties().Where(a => columnArray.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).AsEnumerable();
        List<ColumnKeyInfo> cols = new();
        foreach (PropertyInfo? prop in props)
        {
            ColumnKeyInfo columnInfo = new()
            {
                ModelPropName = prop.Name,
                ColumnName = prop.Name,
            };
            if (prop.GetCustomAttribute(typeof(ColumnAttribute)) is ColumnAttribute columnAttribute && !string.IsNullOrWhiteSpace(columnAttribute.Name))
            {
                columnInfo.ColumnName = columnAttribute.Name;
            }
            cols.Add(columnInfo);
        }
        return cols;
    }
}