using Datalake.Contracts.Models.Data.Values;
using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Sources;

/// <summary>
/// Информация о удалённой записи с данными источника
/// </summary>
public record SourceItemInfo
{
	/// <summary>
	/// Путь к данным в источнике
	/// </summary>
	[Required]
	public required string Path { get; init; }

	/// <summary>
	/// Тип данных
	/// </summary>
	[Required]
	public required TagType Type { get; init; }

	/// <summary>
	/// Значение
	/// </summary>
	[Required]
	public required ValueRecord Value { get; init; }
}
