using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Tags;

/// <summary>
/// Информации о теге, выступающем в качестве входящей переменной при составлении формулы
/// </summary>
public class TagAsInputInfo
{
	/// <summary>
	/// Идентификатор тега в локальной базе
	/// </summary>
	[Required]
	public required int Id { get; set; }

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
}
