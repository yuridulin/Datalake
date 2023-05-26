using LinqToDB.Mapping;

namespace Datalake.Database
{
	[Table(Name = "Sources")]
	public class Source
	{
		[Column, Identity]
		public int Id { get; set; }

		[Column]
		public string Name { get; set; }

		[Column]
		public string Address { get; set; }
	}
}
