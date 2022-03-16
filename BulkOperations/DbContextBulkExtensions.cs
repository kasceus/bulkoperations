using static BulkOperations.Methods.StaticMethods;

using BulkOperations.Models;

using static BulkOperations.Methods.SqlStringMaker;

using System.Data.Entity;


namespace BulkOperations;
/// <summary>
/// Extensions for the DbContext class
/// </summary>
public static class DbContextBulkExtensions
{
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
                string sqlString = MakeString(forUpload, OperationType.Update, options);
                await SqlOperations.RunAsync(ctx, sqlString, forUpload.Count);
            });
            tasks.Add(t);

        }
        Task.WaitAll(tasks.ToArray());
        System.Diagnostics.Debug.WriteLine(tasks);
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
                string sqlString = MakeString(forInsert, OperationType.Insert, options);
                await SqlOperations.RunAsync(ctx, sqlString, forInsert.Count);
            });
            tasks.Add(t);
        }
        Task.WaitAll(tasks.ToArray());
    }/// <summary>
     /// Performs bulk insert or records into the database.
     /// </summary>
     /// <typeparam name="T">Generic type param</typeparam>
     /// <param name="ctx">database context</param>
     /// <param name="data">data to send to the database</param>
     /// <returns></returns>
    public static async Task BulkInsertAsync<T>(this DbContext ctx, IList<T> data)
    {
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
                string sqlString = MakeString(forDelete, OperationType.Delete, options);
                await SqlOperations.RunAsync(ctx, sqlString, forDelete.Count);
            });
            tasks.Add(t);
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
                string sqlString = MakeString(forUpload, OperationType.Update, options);
                await SqlOperations.RunAsync(ctx, sqlString, forUpload.Count);
            });
            tasks.Add(t);

        }
        Task.WaitAll(tasks.ToArray());
        System.Diagnostics.Debug.WriteLine(tasks);
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
    public static void BulkInsert<T>(this DbContext ctx, IList<T> data, Action<BulkOptions> bulkOptions)
    {
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
                string sqlString = MakeString(forInsert, OperationType.Insert, options);
                await SqlOperations.RunAsync(ctx, sqlString, forInsert.Count);
            });
            tasks.Add(t);
        }
        Task.WaitAll(tasks.ToArray());
    }/// <summary>
     /// Performs bulk insert or records into the database.
     /// </summary>
     /// <typeparam name="T">Generic type param</typeparam>
     /// <param name="ctx">database context</param>
     /// <param name="data">data to send to the database</param>
     /// <returns></returns>
    public static void BulkInsert<T>(this DbContext ctx, IList<T> data)
    {
        string[]? keys = GetKeys(data).Result;
        BulkInsert(ctx, data, options => options.KeyNames = keys);
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
    public static void BulkDelete<T>(this DbContext ctx, IList<T> data, Action<BulkOptions> bulkOptions)
    {
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
                string sqlString = MakeString(forDelete, OperationType.Delete, options);
                await SqlOperations.RunAsync(ctx, sqlString, forDelete.Count);
            });
            tasks.Add(t);
        }
        Task.WaitAll(tasks.ToArray());
    }/// <summary>
     /// Performs bulk delete of records from the database.
     /// </summary>
     /// <typeparam name="T">Generic type param</typeparam>
     /// <param name="ctx">database context</param>
     /// <param name="data">data to send to the database</param>
     /// <returns></returns>
    public static void BulkDelete<T>(this DbContext ctx, IList<T> data)
    {
        string[]? keys =GetKeys(data).Result;
        BulkDelete(ctx, data, options => options.KeyNames = keys);
    }
    #endregion
    #endregion
}
