using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Values;

public class ValueTagInfo
{
	[Required]
	public string TagName { get; set; } = string.Empty;

	[Required]
	public TagType TagType { get; set; }
}
