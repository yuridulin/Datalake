namespace Datalake.Server.Services.Receiver.Models.OldDatalake;

internal class HistoryResponse
{
	internal int Id { get; set; }

	internal required string TagName { get; set; }

	internal TagType Type { get; set; }

	internal AggFunc Func { get; set; }

	internal List<HistoryValue> Values { get; set; } = new List<HistoryValue>();
}
