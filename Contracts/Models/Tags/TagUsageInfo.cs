using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Tags;

public record TagUsageInfo
{
	[Required]
	public required int TagId { get; init; }

	[Required]
	public required DateTime Date { get; init; }

	[Required]
	public required string Request { get; init; }
}


public record TagStatusInfo
{
	[Required]
	public required int TagId { get; init; }

	[Required]
	public DateTime Date { get; init; }

	[Required]
	public bool IsError { get; init; }

	public string? Status { get; init; }
}
