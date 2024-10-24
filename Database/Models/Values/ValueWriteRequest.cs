using Datalake.Database.Constants;
using Datalake.Database.Enums;

namespace Datalake.Database.Models.Values;

/// <summary>
/// Данные запроса на ввод значения
/// </summary>
public class ValueWriteRequest
{
	/// <summary>
	/// Глобальные идентификатор тега
	/// </summary>
	public Guid? Guid { get; set; }

	/// <summary>
	/// Идентификатор тега в локальной базе
	/// </summary>
	public int? Id { get; set; }

	/// <summary>
	/// Наименование тега
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Новое значение
	/// </summary>
	public object? Value { get; set; }

	/// <summary>
	/// Дата, на которую будет записано значение
	/// </summary>
	public DateTime? Date { get; set; } = DateFormats.GetCurrentDateTime();

	/// <summary>
	/// Флаг достоверности нового значения
	/// </summary>
	public TagQuality? Quality { get; set; }
}
