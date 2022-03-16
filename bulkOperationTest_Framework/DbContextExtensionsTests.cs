using BulkOperations;
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
            List<BulkTest> records = db.BulkTest.ToList();
            DateTime testDate = DateTime.Now;
            string BigTextField = string.Format("BigTextField Updated at {0}", testDate.ToString());
            string TextField = string.Format("TextField Updated at {0}", testDate.ToString());

            foreach (BulkTest record in records)
            {
                record.BigTextField = string.Format("{0},{1}", record.KeyField, BigTextField);
                record.TextField = TextField;
                record.RandomFiled = "";
            }

            await db.BulkUpdateAsync(records, options => { options.ColumnsToUpdate = new[] { "BigTextField", "TextField" }; });

            Assert.IsTrue(db.BulkTest.Count(a => a.TextField.Equals(TextField)) == records.Count);
        }
        [TestMethod]
        public async Task BulkInsertAsyncTest()
        {
            using DbCtx db = new();
            List<BulkTest> records = new();
            db.BulkTest.RemoveRange(db.BulkTest.ToList());//remove all records
            await db.SaveChangesAsync();
            for (int i = 0; i < 40000; i++)
            {
                int littleStringLength = new Random().Next(10, 50);
                int bigStringLength = new Random().Next(30, 1000);
                records.Add(new BulkTest()
                {
                    TextField = MakeRandomString(littleStringLength),
                    BigTextField = MakeRandomString(bigStringLength)
                });
            }
            await db.BulkInsertAsync(records);
            Assert.IsTrue(records.Count == db.BulkTest.Count());
        }
        private string MakeRandomString(int length)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[length];
            Random random = new();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        [TestMethod()]
        public async Task BulkDeleteAsyncTest()
        {
            using DbCtx db = new();
            List<BulkTest> records = db.BulkTest.Take(10000).ToList();
            int startCount = db.BulkTest.Count();
            await db.BulkDeleteAsync(records);

            Assert.IsTrue(db.BulkTest.Count() == (startCount-10000));
        }
    }
}