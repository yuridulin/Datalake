using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Tags;

public class TagInputInfo
{
	[Required]
	public required int Id { get; set; }

	[Required]
	public required string Name { get; set; }

	[Required]
	public required string VariableName { get; set; }
}
