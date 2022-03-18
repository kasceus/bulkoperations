
using System.Data.Entity;
using System.Data.SqlClient;

using BulkOperations.Exceptions;

namespace BulkOperations.Models;
/// <summary>
/// Holds the run commands for running the commands held by the sql string
/// </summary>
internal class SqlOperations
{
    private int RetryCount = 0;
    private int MinSleep = 500;
    /// <summary>
    /// Run the sql Query and await result
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    internal async Task RunAsync(DbContext context, string SqlString, int totalRecords)
    {
        SqlConnection? conn = new(context.Database.Connection.ConnectionString);
        await conn.OpenAsync();
        SqlTransaction? trans = conn.BeginTransaction();

        try
        {
            SqlCommand sqlCommand = new(SqlString, conn, trans);
            sqlCommand.CommandTimeout = 500;
            await sqlCommand.ExecuteNonQueryAsync();
            trans.Commit();
        }
        catch (SqlException ex) when (ex.Number == 1205){
            if (RetryCount < 10)
            {
                var random = new Random();
                Thread.Sleep(random.Next(MinSleep, 10000));
                await RunAsync(context, SqlString, totalRecords);//retry the transaction on deadlock
            }
            else
            {
                trans.Rollback();
                throw;
            }
            RetryCount++;
            MinSleep += 200;//progressively wait longer between each retry
        }
            catch
        {
            trans.Rollback();
            throw;
        }
        finally
        {
            trans.Dispose();
            conn.Close();
            conn.Dispose();
        }
    }

    /// <summary>
    /// Run the sql Query and await result
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    internal Task Run(DbContext context, string SqlString, int totalRecords)
    {
        SqlConnection? conn = new(context.Database.Connection.ConnectionString);
        conn.Open();
        SqlTransaction? trans = conn.BeginTransaction();

        try
        {
            SqlCommand sqlCommand = new(SqlString, conn, trans);
            sqlCommand.CommandTimeout = 500;
           sqlCommand.ExecuteNonQuery();
            trans.Commit();
        }
        catch (SqlException ex) when (ex.Number == 1205)
        {
            if (RetryCount < 10)
            {
                var random = new Random();
                Thread.Sleep(random.Next(500, 10000));
                Run(context, SqlString, totalRecords);//retry the transaction on deadlock
            }
            else
            {
                trans.Rollback();
                throw;
            }
            RetryCount++;
        }
        catch
        {
            trans.Rollback();
            throw;
        }
        finally
        {
            trans.Dispose();
            conn.Close();
            conn.Dispose();
        }
        return Task.CompletedTask;
    }
}

