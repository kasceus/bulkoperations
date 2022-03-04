namespace bulkOperationTest_Framework.Models
{
	using System.ComponentModel.DataAnnotations;
	using System.ComponentModel.DataAnnotations.Schema;

	[Table("tbl_BulkTest")]
	public partial class BulkTest
	{
		[Key]
		[Column("Id")]
		public int KeyField { get; set; }

		[StringLength(50)]
		public string TextField { get; set; }

		public string BigTextField { get; set; }
	}
}
