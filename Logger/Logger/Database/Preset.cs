using LinqToDB.Mapping;

namespace Logger.Database
{
	[Table(Name = "Presets")]
	public class Preset
	{
		[Column, Identity]
		public int Id { get; set; }

		[Column, NotNull]
		public string Name { get; set; }

		[Column]
		public string Description { get; set; }
	}
}
