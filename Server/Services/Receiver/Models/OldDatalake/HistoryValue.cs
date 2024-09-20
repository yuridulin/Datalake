namespace Datalake.Server.Services.Receiver.Models.OldDatalake;

internal class HistoryValue
{
	internal DateTime Date { get; set; } = DateTime.Now;

	internal object? Value { get; set; }

	internal TagQuality Quality { get; set; }

	internal TagHistoryUse Using { get; set; }
}
