using LinqToDB.Mapping;

namespace Logger.Database
{
	[Table(Name = "StationsSpecs")]
	public class StationSpec
	{
		[Column]
		public int Id { get;set; }

		[Column]
		public string Endpoint { get; set; }

		[Column]
		public string Page { get; set; }

		[Column]
		public string Device { get; set; }

		[Column]
		public string ItemGroup { get; set; }

		[Column]
		public string ItemId { get; set; }

		[Column]
		public string Item { get; set; }

		[Column]
		public string Value { get; set; }

		// 

		public int LocalId { get; set; } = 0;
	}
}
