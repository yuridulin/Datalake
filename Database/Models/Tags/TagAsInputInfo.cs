using Datalake.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Tags;

/// <summary>
/// Информации о теге, выступающем в качестве входящей переменной при составлении формулы
/// </summary>
public class TagAsInputInfo : TagSimpleInfo
{
	/// <summary>
	/// Тип данных тега
	/// </summary>
	[Required]
	public required TagType Type { get; set; }
}
