using Datalake.Database.Enums;
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

		[Column]
		public TagType TagType { get; set; }

		[Column]
		public decimal MinEU { get; set; } = 0;

		[Column]
		public decimal MaxEU { get; set; } = 100;

		[Column]
		public decimal MinScale { get; set; } = decimal.MinValue;

		[Column]
		public decimal MaxScale { get; set; } = decimal.MaxValue;
	}
}
