using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Sources;

public class SourceItemInfo
{
	[Required]
	public required string Path { get; set; }

	[Required]
	public TagType Type { get; set; }
}
