using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Values;

public class ValueTagInfo
{
	[Required]
	public string TagName { get; set; } = string.Empty;

	[Required]
	public TagType TagType { get; set; }
}
