using Datalake.Contracts.Interfaces;
using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Tags;

/// <summary>
/// Базовая информация о теге, достаточная, чтобы на него сослаться
/// </summary>
public class TagSimpleInfo : IProtectedEntity
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
	/// Произвольное описание тега
	/// </summary>
	public string? Description { get; set; }

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
	/// Идентификатор источника данных
	/// </summary>
	[Required]
	public required int SourceId { get; set; }

	/// <summary>
	/// Тип данных источника
	/// </summary>
	[Required]
	public required SourceType SourceType { get; set; } = SourceType.Datalake;

	/// <summary>
	/// Доступ к данному тегу
	/// </summary>
	[Required]
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
