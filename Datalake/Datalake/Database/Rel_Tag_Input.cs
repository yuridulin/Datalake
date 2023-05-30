using LinqToDB.Mapping;

namespace Datalake.Database
{
	[Table(Name = "Rel_Tag_Input")]
	public class Rel_Tag_Input
	{
		[Column, NotNull]
		public int TagId { get; set; } = 0;

		[Column, NotNull]
		public int InputTagId { get; set; } = 0;

		[Column]
		public string VariableName { get; set; } = string.Empty;
	}
}