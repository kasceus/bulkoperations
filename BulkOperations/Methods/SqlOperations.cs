
using System.Data.Entity;
using System.Data.SqlClient;

using BulkOperations.Exceptions;

namespace BulkOperations.Models;
/// <summary>
/// Holds the run commands for running the commands held by the sql string
/// </summary>
internal static class SqlOperations
{
	/// <summary>
	/// Run the sql Query and await result
	/// </summary>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	internal static async Task RunAsync(DbContext context, string SqlString, int totalRecords)
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
	internal static Task Run(DbContext context, string SqlString, int totalRecords)
	{
		SqlConnection? conn = new(context.Database.Connection.ConnectionString);
		conn.Open();
		SqlTransaction? trans = conn.BeginTransaction();
		try
		{
			SqlCommand sqlCommand = new(SqlString, conn);
			int changed = sqlCommand.ExecuteNonQuery();
			if (changed == totalRecords)
			{
				trans.Commit();
			}
			else
			{
				trans.Rollback();
			}
		}
		catch
		{
			trans.Rollback();
		}
		conn.Close();
		conn.Dispose();
		return Task.CompletedTask;
	}
}

