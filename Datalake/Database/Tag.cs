using LinqToDB.Mapping;

namespace Datalake.Database
{
	[Table(Name = "Tags")]
	public class Tag
	{
		[Column, PrimaryKey]
		public string TagName { get; set; } = string.Empty;

		[Column]
		public string Description { get; set; } = string.Empty;

		[Column]
		public int SourceId { get; set; }

		[Column]
		public string SourceItem { get; set; }

		[Column]
		public short Interval { get; set; }
	}
}
