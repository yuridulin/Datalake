namespace Datalake.Server.Services.Receiver.Models.OldDatalake;

public enum TagHistoryUse
{
	Initial = 0,
	Basic = 1,
	Aggregated = 2,
	Continuous = 3,
	Outdated = 100,
	NotFound = 101,
}
