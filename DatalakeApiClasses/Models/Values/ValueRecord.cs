using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Values;

public class ValueRecord
{
	[Required]
	public required DateTime Date { get; set; }

	[Required]
	public required string DateString { get; set; }

	[Required]
	public required object? Value { get; set; }

	[Required]
	public required TagQuality Quality { get; set; }

	[Required]
	public required TagUsing Using { get; set; }
}
