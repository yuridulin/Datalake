using Datalake.Contracts.Public.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Public.Models.Data.Values;

/// <summary>
/// Краткая информация о запрашиваемом теге
/// </summary>
public class ValueTagInfo
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
	public required string TagName { get; set; }

	/// <summary>
	/// Тип данных значений тега
	/// </summary>
	[Required]
	public TagType TagType { get; set; }
}
