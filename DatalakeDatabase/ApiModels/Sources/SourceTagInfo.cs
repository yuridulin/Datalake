using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Sources;

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
