using Datalake.Contracts.Public.Enums;

namespace Datalake.Data.Api.Models.Values;

/// <summary>
/// Данные запроса на ввод значения
/// </summary>
public record ValueWriteRequest
{
	/// <summary>
	/// Идентификатор тега
	/// </summary>
	public required int Id { get; init; }

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
