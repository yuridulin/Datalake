using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Logs;

public class LogInfo
{
	[Required]
	public required long Id { get; set; }

	/// <summary>
	/// Дата формата DateFormats.Long
	/// </summary>
	[Required]
	public required string DateString { get; set; }

	[Required]
	public required LogCategory Category { get; set; }

	[Required]
	public required LogType Type { get; set; }

	[Required]
	public required string Text { get; set; }

	public string? RefId { get; set; }
}
