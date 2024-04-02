using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Sources
{
	public class SourceInfo
	{
		public int Id { get; set; }

		public required string Name { get; set; }

		public string? Description { get; set; }

		public string? Address { get; set; }

		public SourceType Type { get; set; }
	}
}
