using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Tags;

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
}
