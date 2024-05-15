using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Sources;

public class SourceInfo
{
	[Required]
	public int Id { get; set; }

	[Required]
	public required string Name { get; set; }

	public string? Description { get; set; }

	public string? Address { get; set; }

	[Required]
	public SourceType Type { get; set; }
}
