using Datalake.Enums;
using LinqToDB.Mapping;

namespace Datalake.Database.V0
{
	[Table(Name = "Sources")]
	public class Source
	{
		[Column, Identity]
		public int Id { get; set; }

		[Column]
		public string Name { get; set; }

		[Column]
		public SourceType Type { get; set; }

		[Column]
		public string Address { get; set; }
	}
}
