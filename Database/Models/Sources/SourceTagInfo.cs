using Datalake.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Sources;

/// <summary>
/// Информация о теге, берущем данные из этого источника
/// </summary>
public class SourceTagInfo
{
	/// <summary>
	/// Идентификатор тега
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Глобальное наименование тега
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Путь к данным в источнике
	/// </summary>
	[Required]
	public required string Item { get; set; }

	/// <summary>
	/// Тип данных тега
	/// </summary>
	[Required]
	public required TagType Type { get; set; }

	/// <summary>
	/// Интервал обновления тега
	/// </summary>
	[Required]
	public required int Interval { get; set; }
}
