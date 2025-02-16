using Datalake.Database.Abstractions;
using Datalake.Database.Enums;
using Datalake.Database.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Sources;

/// <summary>
/// Информация о теге, берущем данные из этого источника
/// </summary>
public class SourceTagInfo : IProtectedEntity
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
	/// Частота записи значений тега
	/// </summary>
	[Required]
	public required TagFrequency Frequency { get; set; }

	/// <inheritdoc />
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
