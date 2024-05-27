using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Sources;

public class SourceTagInfo
{
	[Required]
	public int Id { get; set; }

	[Required]
	public required string Name { get; set; }

	[Required]
	public required string Item { get; set; }

	[Required]
	public TagType Type { get; set; }
}
