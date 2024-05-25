using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Tags;

public class TagInputInfo
{
	[Required]
	public required int Id { get; set; }

	[Required]
	public required string Name { get; set; }

	[Required]
	public required string VariableName { get; set; }
}
