namespace Datalake.PublicApi.Models.Tables;

/// <summary>
/// Информация о таблице, включая название индекса по TagId и Date
/// </summary>
public class HistoryTableWithIndex
{
	/// <summary>
	/// Название таблицы
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Название индекса таблицы, если есть
	/// </summary>
	public string? Index { get; set; } = null;
}
