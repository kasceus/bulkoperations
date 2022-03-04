using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using bulkOperationTest_Framework.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BulkOperations.Tests
{
	[TestClass()]
	public class DbContextExtensionsTests
	{
		[TestMethod()]
		public async Task BulkUpdateAsyncTestAsync()
		{
			using DbCtx db = new();
			List<BulkTest> records = db.BulkTest.Take(10000).ToList();
			DateTime testDate = DateTime.Now;

			for (int i = 0; i < 10000; i++)
			{
				int littleStringLength = new Random().Next(50);
				int bigStringLength = new Random().Next(1000);
				records.Add(new BulkTest()
				{
					TextField = MakeRandomString(littleStringLength),
					BigTextField = MakeRandomString(bigStringLength)
				});
			}
			//foreach (BulkTest? record in records)
			//{
			//	record.BigTextField = string.Format("BigTextField Updated at {0}", testDate.ToString());
			//	record.TextField = string.Format("TextField Updated at {0}", testDate.ToString());
			//}

			await db.BulkUpdateAsync(records, options => { options.UseTransactions = false; options.ColumnsToUpdate = new[] { "TextField", "BigTextField" }; });

			Assert.IsTrue(db.BulkTest.Count() == records.Count);
		}
		public async Task BulkInsertAsyncTest()
		{
			using DbCtx db = new();
			List<BulkTest> records = new();
			db.BulkTest.RemoveRange(db.BulkTest.ToList());//remove all records
			await db.SaveChangesAsync();
			for (int i = 0; i < 10000; i++)
			{
				int littleStringLength = new Random().Next(50);
				int bigStringLength = new Random().Next(1000);
				records.Add(new BulkTest()
				{
					TextField = MakeRandomString(littleStringLength),
					BigTextField = MakeRandomString(bigStringLength)
				});
			}
			Assert.IsTrue(records.Count == db.BulkTest.Count());
		}
		private string MakeRandomString(int length)
		{
			string? chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			char[]? stringChars = new char[length];
			Random? random = new Random();

			for (int i = 0; i < stringChars.Length; i++)
			{
				stringChars[i] = chars[random.Next(chars.Length)];
			}

			return new string(stringChars);
		}
	}
}