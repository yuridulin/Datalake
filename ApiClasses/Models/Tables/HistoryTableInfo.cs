namespace Datalake.ApiClasses.Models.Tables;

/// <summary>
/// Информация о таблице истории
/// </summary>
public class HistoryTableInfo
{
	/// <summary>
	/// Название таблицы
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Дата суток, на которые значения
	/// </summary>
	public required DateTime Date { get; set; }

	/// <summary>
	/// Найден ли индекс в базе данных
	/// </summary>
	public required bool HasIndex { get; set; }
}
