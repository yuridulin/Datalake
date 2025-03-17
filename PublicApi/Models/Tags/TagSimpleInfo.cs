using Datalake.PublicApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Tags;

/// <summary>
/// Базовая информация о теге, достаточная, чтобы на него сослаться
/// </summary>
public class TagSimpleInfo
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
	public required TagFrequency Frequency { get; set; }

	/// <summary>
	/// Тип данных источника
	/// </summary>
	[Required]
	public required SourceType SourceType { get; set; } = SourceType.Datalake;
}
