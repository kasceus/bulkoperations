#if NET6_0_OR_GREATER
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

#else
using System.Data.Entity;
using System.Data.SqlClient;

using BulkOperations.Exceptions;
#endif


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
#if NET6_0_OR_GREATER
		SqlConnection? conn = new(context.Database.GetDbConnection().ConnectionString);
		await conn.OpenAsync();
		SqlTransaction? trans = conn.BeginTransaction();
		try
		{
			SqlCommand sqlCommand = new(SqlString, conn, trans);
			sqlCommand.BeginExecuteNonQuery();
			int changed = await sqlCommand.ExecuteNonQueryAsync();

			if (changed == totalRecords)
			{
				await trans.CommitAsync();
				sqlCommand.Dispose();
			}
			else
			{
				await trans.RollbackAsync();
			}
		}
		catch
		{
			await trans.RollbackAsync();
			throw;
		}
		finally
		{

			await trans.DisposeAsync();
			await conn.CloseAsync();
			await conn.DisposeAsync();
			trans = null;
			conn = null;
		}



#else
		SqlConnection? conn = new(context.Database.Connection.ConnectionString);
		await conn.OpenAsync();
		SqlTransaction? trans = conn.BeginTransaction();

		try
		{
			//TODO: get with eugene to see what's actually bein executed here so i know why it's nullifying records
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
			trans = null;
			conn = null;
		}
#endif

	}
#if NET6_0_OR_GREATER
	/// <summary>
	/// Run the sql Query and await result
	/// </summary>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	internal static Task Run(DbContext context, string SqlString, int totalRecords)
	{
		SqlConnection? conn = new(context.Database.GetDbConnection().ConnectionString);
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
#else
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
#endif
}

