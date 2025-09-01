using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Values;

/// <summary>
/// Ответ на запрос для получения значений, характеризующий запрошенный тег и его значения
/// </summary>
public class ValuesTagResponse : TagSimpleInfo
{
	/// <summary>
	/// Список значений
	/// </summary>
	[Required]
	public required ValueRecord[] Values { get; set; } = [];

	/// <summary>
	/// Как прошла операция
	/// </summary>
	[Required]
	public ValueResult Result { get; set; }
}
