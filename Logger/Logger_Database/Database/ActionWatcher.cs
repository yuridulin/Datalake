using LinqToDB.Mapping;

namespace Logger.Database
{
	[Table(Name = "ActionsWatchers")]
	public class ActionWatcher
	{
		[Column, Identity]
		public int Id { get; set; }

		[Column]
		public string Name { get; set; }

		[Column]
		public int WatcherId { get; set; }

		[Column]
		public string Comparer { get; set; }

		[Column]
		public int Value { get; set; }

		[Column]
		public string Template { get; set; }
	}
}
