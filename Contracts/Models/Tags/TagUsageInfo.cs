namespace Datalake.Contracts.Models.Tags;

public record TagUsageInfo(
	int TagId,
	DateTime Date,
	string Request);

public record TagStatusInfo(
	int TagId,
	DateTime Date,
	bool IsError,
	string? Status);
