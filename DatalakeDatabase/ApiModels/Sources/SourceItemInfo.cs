using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Sources;

public class SourceItemInfo
{
	[Required]
	public required string Path { get; set; }

	[Required]
	public TagType Type { get; set; }
}
