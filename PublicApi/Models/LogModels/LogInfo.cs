using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.LogModels;

/// <summary>
/// Запись собщения
/// </summary>
public class LogInfo
{
	/// <summary>
	/// Идентификатор записи
	/// </summary>
	[Required]
	public required long Id { get; set; }

	/// <summary>
	/// Дата формата DateFormats.Long
	/// </summary>
	[Required]
	public required string DateString { get; set; }

	/// <summary>
	/// Категория сообщения (к какому объекту относится)
	/// </summary>
	[Required]
	public required LogCategory Category { get; set; }

	/// <summary>
	/// Степень важности сообщения
	/// </summary>
	[Required]
	public required LogType Type { get; set; }

	/// <summary>
	/// Текст сообщеня
	/// </summary>
	[Required]
	public required string Text { get; set; }

	/// <summary>
	/// Ссылка на конкретный объект в случае, если это подразумевает категория
	/// <br />
	/// Теги, пользователи, группы пользователей: Guid
	/// Источники, блоки: int
	/// </summary>
	public string? RefId { get; set; }

	/// <summary>
	/// Информация об авторе сообщения
	/// </summary>
	public UserSimpleInfo? Author { get; set; }
}
