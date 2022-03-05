using System.Data.Entity;

namespace bulkOperationTest_Framework.Models
{
	/// <summary>
	///
	/// </summary>
	public partial class DbCtx : DbContext
	{
		public DbCtx()
			: base("name=TestContext")
		{
			Configuration.AutoDetectChangesEnabled = false;
			Configuration.ProxyCreationEnabled = false;
		}

		public virtual DbSet<BulkTest> BulkTest { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{

		}
	}
}
