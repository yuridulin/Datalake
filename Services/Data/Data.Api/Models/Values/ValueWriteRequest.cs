using Datalake.Contracts.Public.Enums;

namespace Datalake.Data.Api.Models.Values;

/// <summary>
/// Данные запроса на ввод значения
/// </summary>
public record ValueWriteRequest
{
	/// <summary>
	/// Локальный идентификатор тега
	/// </summary>
	public int? Id { get; init; }

	/// <summary>
	/// Глобальный идентификатор тега
	/// </summary>
	public Guid? Guid { get; init; }

	/// <summary>
	/// Новое значение, не типизированое
	/// </summary>
	public object? Value { get; init; }

	/// <summary>
	/// Дата, на которую будет записано значение
	/// </summary>
	public DateTime? Date { get; init; }

	/// <summary>
	/// Флаг достоверности нового значения
	/// </summary>
	public TagQuality? Quality { get; init; }
}
