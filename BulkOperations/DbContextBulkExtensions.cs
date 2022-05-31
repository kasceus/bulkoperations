using static BulkOperations.Methods.StaticMethods;

using BulkOperations.Models;

using static BulkOperations.Methods.SqlStringMaker;

using System.Data.Entity;
using System.Diagnostics;

namespace BulkOperations;
/// <summary>
/// Extensions for the DbContext class
/// </summary>
public static class DbContextBulkExtensions
{
    private static int iteration = 0;
    #region Async Methods
    #region Update
    /// <summary>
    /// Update a list of items asynchronously
    /// </summary>
    /// <typeparam name="T">Type of object</typeparam>
    /// <param name="ctx">db context</param>
    /// <param name="data">Data that needs to be updated</param>
    /// <param name="bulkOptions">Options for handling the bulk operation</param>
    /// <returns></returns>
    public static async Task BulkUpdateAsync<T>(this DbContext ctx, IList<T> data, Action<BulkOptions> bulkOptions)
    {
        if (data.Count == 0)
        {
            return;
        }
        BulkOptions options = new();
        bulkOptions?.Invoke(options);
        if (options.KeyNames == null)
        {
            options.KeyNames = await GetKeys(data);
        }
        List<Task> tasks = new();
        List<T> ToUplaod = new();
        ToUplaod.AddRange(data);
        while (ToUplaod.Count > 0) //perform the bulk operations here
        {
            List<T> forUpload = new();
            lock (ToUplaod)
            {
                if (ToUplaod.Count > options.BatchSize)
                {
                    forUpload.AddRange(ToUplaod.Take(options.BatchSize));
                    ToUplaod.RemoveAll(a => forUpload.Contains(a));
                }
                else
                {
                    forUpload.AddRange(ToUplaod);
                    ToUplaod.Clear();
                }
            }
            Task t = Task.Run(async () =>
            {
                Thread.Sleep(new Random().Next(250, 1000));//add random sleep so the concurrent operations don't lock up the database
                string sqlString = MakeString(forUpload, OperationType.Update, options, iteration);
                SqlOperations sql = new();
                await sql.RunAsync(ctx, sqlString, forUpload.Count);
            });
            tasks.Add(t);
            iteration++;
        }
        Task.WaitAll(tasks.ToArray());
        return;
    }
    /// <summary>
    /// Update a list of items asynchronously
    /// </summary>
    /// <typeparam name="T">Type of object</typeparam>
    /// <param name="ctx">db context</param>
    /// <param name="data">Data that needs to be updated</param>
    /// <returns></returns>
    public static async Task BulkUpdateAsync<T>(this DbContext ctx, IList<T> data)
    {
        if (data.Count == 0)
        {
            return;
        }
        string[]? KeyNames = await GetKeys(data);
        await BulkUpdateAsync(ctx, data, option => option.KeyNames = KeyNames);
    }

    #endregion
    #region Insert
    /// <summary>
    /// Perform a bulk insert of records
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ctx"></param>
    /// <param name="data"></param>
    /// <param name="bulkOptions"></param>
    /// <returns></returns>
    public static async Task BulkInsertAsync<T>(this DbContext ctx, IList<T> data, Action<BulkOptions> bulkOptions)
    {

        if (data.Count == 0)
        {
            return;
        }
        BulkOptions options = new();
        bulkOptions?.Invoke(options);
        if (options.KeyNames == null)
        {
            options.KeyNames = await GetKeys(data);
        }
        List<Task> tasks = new();
        List<T> ToInsert = new();
        ToInsert.AddRange(data);
        while (ToInsert.Count > 0) //perform the bulk operations here
        {
            List<T> forInsert = new();
            lock (ToInsert)
            {
                if (ToInsert.Count > options.BatchSize)
                {
                    forInsert.AddRange(ToInsert.Take(options.BatchSize));
                    ToInsert.RemoveAll(a => forInsert.Contains(a));
                }
                else
                {
                    forInsert.AddRange(ToInsert);
                    ToInsert.Clear();
                }
            }
            Task t = Task.Run(async () =>
            {
                string sqlString = MakeString(forInsert, OperationType.Insert, options, iteration);
                try
                {
                    SqlOperations sql = new();
                    await sql.RunAsync(ctx, sqlString, forInsert.Count);                    
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw new Exception(ex.Message);
                }
            });
            tasks.Add(t);
            iteration++;
        }

        Task.WaitAll(tasks.ToArray());
        return;
    }/// <summary>
     /// Performs bulk insert or records into the database.
     /// </summary>
     /// <typeparam name="T">Generic type param</typeparam>
     /// <param name="ctx">database context</param>
     /// <param name="data">data to send to the database</param>
     /// <returns></returns>
    public static async Task BulkInsertAsync<T>(this DbContext ctx, IList<T> data)
    {
        if (data.Count == 0)
        {
            return;
        }
        string[]? keys = await GetKeys(data);
        await BulkInsertAsync(ctx, data, options => options.KeyNames = keys);
    }
    #endregion
    #region Delete
    /// <summary>
    /// Perform a bulk delete of records
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ctx"></param>
    /// <param name="data"></param>
    /// <param name="bulkOptions"></param>
    /// <returns></returns>
    public static async Task BulkDeleteAsync<T>(this DbContext ctx, IList<T> data, Action<BulkOptions> bulkOptions)
    {

        if (data.Count == 0)
        {
            return;
        }
        BulkOptions options = new();
        bulkOptions?.Invoke(options);
        if (options.KeyNames == null)
        {
            options.KeyNames = await GetKeys(data);
        }
        List<Task> tasks = new();
        List<T> toDelete = new();
        toDelete.AddRange(data);
        while (toDelete.Count > 0) //perform the bulk operations here
        {
            List<T> forDelete = new();
            lock (toDelete)
            {
                if (toDelete.Count > options.BatchSize)
                {
                    forDelete.AddRange(toDelete.Take(options.BatchSize));
                    toDelete.RemoveAll(a => forDelete.Contains(a));
                }
                else
                {
                    forDelete.AddRange(toDelete);
                    toDelete.Clear();
                }
            }
            Task t = Task.Run(async () =>
            {
                string sqlString = MakeString(forDelete, OperationType.Delete, options, iteration);
                SqlOperations sql = new();
                await sql.RunAsync(ctx, sqlString, forDelete.Count);
            });
            tasks.Add(t);
            iteration++;
        }
        Task.WaitAll(tasks.ToArray());
    }/// <summary>
     /// Performs bulk delete of records from the database.
     /// </summary>
     /// <typeparam name="T">Generic type param</typeparam>
     /// <param name="ctx">database context</param>
     /// <param name="data">data to send to the database</param>
     /// <returns></returns>
    public static async Task BulkDeleteAsync<T>(this DbContext ctx, IList<T> data)
    {
        if (data.Count == 0)
        {
            return;
        }
        string[]? keys = await GetKeys(data);
        await BulkDeleteAsync(ctx, data, options => options.KeyNames = keys);
    }
    #endregion
    #endregion
    #region Synchronous Methods
    #region update
    /// <summary>
    /// Update a list of items asynchronously
    /// </summary>
    /// <typeparam name="T">Type of object</typeparam>
    /// <param name="ctx">db context</param>
    /// <param name="data">Data that needs to be updated</param>
    /// <param name="bulkOptions">Options for handling the bulk operation</param>
    /// <returns></returns>
    public static void BulkUpdate<T>(this DbContext ctx, IList<T> data, Action<BulkOptions> bulkOptions)
    {

        if (data.Count == 0)
        {
            return;
        }
        BulkOptions options = new();
        bulkOptions?.Invoke(options);
        if (options.KeyNames == null)
        {
            options.KeyNames = GetKeys(data).Result;
        }
        List<Task> tasks = new();
        List<T> ToUplaod = new();
        ToUplaod.AddRange(data);
        while (ToUplaod.Count > 0) //perform the bulk operations here
        {
            List<T> forUpload = new();
            lock (ToUplaod)
            {
                if (ToUplaod.Count > options.BatchSize)
                {
                    forUpload.AddRange(ToUplaod.Take(options.BatchSize));
                    ToUplaod.RemoveAll(a => forUpload.Contains(a));
                }
                else
                {
                    forUpload.AddRange(ToUplaod);
                    ToUplaod.Clear();
                }
            }
            Task t = Task.Run(async () =>
            {
                Thread.Sleep(new Random().Next(250, 1000));//add random sleep so the concurrent operations don't lock up the database
                string sqlString = MakeString(forUpload, OperationType.Update, options, iteration);
                SqlOperations sql = new();
                await sql.RunAsync(ctx, sqlString, forUpload.Count);
            });
            tasks.Add(t);
            iteration++;
        }
        Task.WaitAll(tasks.ToArray());
    }
    /// <summary>
    /// Update a list of items asynchronously
    /// </summary>
    /// <typeparam name="T">Type of object</typeparam>
    /// <param name="ctx">db context</param>
    /// <param name="data">Data that needs to be updated</param>
    /// <returns></returns>
    public static void BulkUpdate<T>(this DbContext ctx, IList<T> data)
    {
        if (data.Count == 0)
        {
            return;
        }
        string[]? KeyNames = GetKeys(data).Result;
        BulkUpdate(ctx, data, option => option.KeyNames = KeyNames);
    }
    #endregion
    #region Insert
    /// <summary>
    /// Perform a bulk insert of records
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ctx"></param>
    /// <param name="data"></param>
    /// <param name="bulkOptions"></param>
    /// <returns></returns>
    public static bool BulkInsert<T>(this DbContext ctx, IList<T> data, Action<BulkOptions> bulkOptions)
    {

        if (data.Count == 0)
        {
            return true;
        }
        BulkOptions options = new();
        bulkOptions?.Invoke(options);
        if (options.KeyNames == null)
        {
            options.KeyNames = GetKeys(data).Result;
        }
        List<Task> tasks = new();
        List<T> ToInsert = new();
        ToInsert.AddRange(data);
        while (ToInsert.Count > 0) //perform the bulk operations here
        {
            List<T> forInsert = new();
            lock (ToInsert)
            {
                if (ToInsert.Count > options.BatchSize)
                {
                    forInsert.AddRange(ToInsert.Take(options.BatchSize));
                    ToInsert.RemoveAll(a => forInsert.Contains(a));
                }
                else
                {
                    forInsert.AddRange(ToInsert);
                    ToInsert.Clear();
                }
            }
            Task t = Task.Run(async () =>
            {
                Thread.Sleep(new Random().Next(250, 1000));//add random sleep so the concurrent operations don't lock up the database
                string sqlString = MakeString(forInsert, OperationType.Insert, options, iteration);
                SqlOperations sql = new();
                await sql.RunAsync(ctx, sqlString, forInsert.Count);
            });
            tasks.Add(t);
            iteration++;
        }
        Task.WaitAll(tasks.ToArray());
        return true;
    }/// <summary>
     /// Performs bulk insert or records into the database.
     /// </summary>
     /// <typeparam name="T">Generic type param</typeparam>
     /// <param name="ctx">database context</param>
     /// <param name="data">data to send to the database</param>
     /// <returns></returns>
    public static bool BulkInsert<T>(this DbContext ctx, IList<T> data)
    {
        if (data.Count == 0)
        {
            return true;
        }
        string[]? keys = GetKeys(data).Result;
        return BulkInsert(ctx, data, options => options.KeyNames = keys);
    }
    #endregion
    #region Delete
    /// <summary>
    /// Perform a bulk delete of records
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ctx"></param>
    /// <param name="data"></param>
    /// <param name="bulkOptions"></param>
    /// <returns></returns>
    public static bool BulkDelete<T>(this DbContext ctx, IList<T> data, Action<BulkOptions> bulkOptions)
    {

        if (data.Count == 0)
        {
            return true;
        }
        BulkOptions options = new();
        bulkOptions?.Invoke(options);
        if (options.KeyNames == null)
        {
            options.KeyNames = GetKeys(data).Result;
        }
        List<Task> tasks = new();
        List<T> toDelete = new();
        toDelete.AddRange(data);
        while (toDelete.Count > 0) //perform the bulk operations here
        {
            List<T> forDelete = new();
            lock (toDelete)
            {
                if (toDelete.Count > options.BatchSize)
                {
                    forDelete.AddRange(toDelete.Take(options.BatchSize));
                    toDelete.RemoveAll(a => forDelete.Contains(a));
                }
                else
                {
                    forDelete.AddRange(toDelete);
                    toDelete.Clear();
                }
            }
            Task t = Task.Run(async () =>
            {
                Thread.Sleep(new Random().Next(250, 1000));//add random sleep so the concurrent operations don't lock up the database
                string sqlString = MakeString(forDelete, OperationType.Delete, options, iteration);
                SqlOperations sql = new();
                await sql.RunAsync(ctx, sqlString, forDelete.Count);
            });
            tasks.Add(t);
            iteration++;
        }
        Task.WaitAll(tasks.ToArray());
        return true;
    }
    /// <summary>
    /// Performs bulk delete of records from the database.
    /// </summary>
    /// <typeparam name="T">Generic type param</typeparam>
    /// <param name="ctx">database context</param>
    /// <param name="data">data to send to the database</param>
    /// <returns></returns>
    public static bool BulkDelete<T>(this DbContext ctx, IList<T> data)
    {
        if (data.Count == 0)
        {
            return true;
        }
        string[]? keys = GetKeys(data).Result;
        return BulkDelete(ctx, data, options => options.KeyNames = keys);
    }
    #endregion
    #endregion
}
