using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Logs;

public class LogInfo
{
	public required long Id { get; set; }

	public required DateTime Date { get; set; }

	public required LogCategory Category { get; set; }

	public required LogType Type { get; set; }

	public required string Text { get; set; }

	public int? RefId { get; set; }
}
