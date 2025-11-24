namespace Datalake.Contracts.Models.Tags;

public record TagUsageInfo(
	int TagId,
	Dictionary<string, DateTime> Requests);

public record TagStatusInfo(
	int TagId,
	DateTime Date,
	bool IsError,
	string? Status);
