namespace Datalake.Server.Services.Receiver.Models.OldDatalake;

public class HistoryResponse
{
	public int Id { get; set; }

	public required string TagName { get; set; }

	public TagType Type { get; set; }

	public AggFunc Func { get; set; }

	public List<HistoryValue> Values { get; set; } = new List<HistoryValue>();
}
