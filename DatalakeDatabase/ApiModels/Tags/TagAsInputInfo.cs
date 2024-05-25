using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Tags;

public class TagAsInputInfo
{
	[Required]
	public required int Id { get; set; }

	[Required]
	public required string Name { get; set; }

	[Required]
	public required TagType Type { get; set; }
}
