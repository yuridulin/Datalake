using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Sources;

public class SourceItemInfo
{
	public required string Path { get; set; }

	public TagType Type { get; set; }
}
