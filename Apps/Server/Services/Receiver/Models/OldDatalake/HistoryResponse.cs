namespace Datalake.Server.Services.Receiver.Models.OldDatalake;

/// <summary>
/// Ответ с данными
/// </summary>
public class HistoryResponse
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Имя тега (путь)
	/// </summary>
	public required string TagName { get; set; }

	/// <summary>
	/// Тип
	/// </summary>
	public TagType Type { get; set; }

	/// <summary>
	/// Выбранная агрегирующая функция
	/// </summary>
	public AggFunc Func { get; set; }

	/// <summary>
	/// Значения
	/// </summary>
	public List<HistoryValue> Values { get; set; } = [];
}
