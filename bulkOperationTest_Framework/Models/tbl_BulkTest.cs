namespace bulkOperationTest_Framework.Models
{
	using System.ComponentModel.DataAnnotations;
	using System.ComponentModel.DataAnnotations.Schema;

	[Table("tbl_BulkTest")]
	public partial class BulkTest
	{
		/// <summary>
		///
		/// </summary>
		public BulkTest()
		{

		}
		[Key]
		[Column("Id")]
		public int KeyField { get; set; } = default!;

		[StringLength(50)]
		public string TextField { get; set; } = default!;

		public string BigTextField { get; set; } = default!;
		public string RandomFiled { get; set; }= default!;
	}
}
