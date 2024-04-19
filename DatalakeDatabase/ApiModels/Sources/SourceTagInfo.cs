using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Sources;

public class SourceTagInfo
{
	public int Id { get; set; }

	public required string Name { get; set; }

	public required string Item { get; set; }

	public TagType Type { get; set; }
}
