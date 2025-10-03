using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Data.Api.Models.Values;

/// <summary>
/// Данные запроса на ввод значения
/// </summary>
public class ValueTrustedWriteRequest
{
	/// <summary>
	/// Идентификатор тега в локальной базе
	/// </summary>
	[Required]
	public required int Id { get; set; }

	/// <summary>
	/// Глобальный идентификатор тега
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Имя тега
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Тип данных тега
	/// </summary>
	[Required]
	public required TagType Type { get; set; }

	/// <summary>
	/// Частота записи тега
	/// </summary>
	[Required]
	public required TagResolution Resolution { get; set; }

	/// <summary>
	/// Тип данных источника
	/// </summary>
	[Required]
	public required SourceType SourceType { get; set; } = SourceType.Datalake;

	/// <summary>
	/// Коэффициент преобразования (соотношение новой и исходной шкал, заданных в настройках тега)
	/// </summary>
	[Required]
	public required float ScalingCoefficient { get; set; } = 1;

	/// <summary>
	/// Тег отмечен как удаленный
	/// </summary>
	[Required]
	public required bool IsDeleted { get; set; } = false;

	/// <summary>
	/// Идентификатор источника данных тега
	/// </summary>
	[Required]
	public required int SourceId { get; set; }

	/// <summary>
	/// Новое значение
	/// </summary>
	public object? Value { get; set; }

	/// <summary>
	/// Дата, на которую будет записано значение
	/// </summary>
	public DateTime? Date { get; set; } = DateTimeExtension.GetCurrentDateTime();

	/// <summary>
	/// Флаг достоверности нового значения
	/// </summary>
	public TagQuality? Quality { get; set; }
}
