using LinqToDB.Mapping;

namespace Logger.Database
{
	[Table(Name = "ActionsPings")]
	public class ActionPing
	{
		[Column, Identity]
		public int Id { get; set; }

		[Column]
		public string Name { get; set; }

		[Column]
		public string Template { get; set; }

		[Column]
		public string Target { get; set; }

		[Column]
		public int Interval { get; set; }

		[Column]
		public int Value { get; set; }
	}
}
