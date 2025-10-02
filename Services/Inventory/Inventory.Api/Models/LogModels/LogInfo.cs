using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Inventory.Api.Models.Sources;
using Datalake.Inventory.Api.Models.Tags;
using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Inventory.Api.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.LogModels;

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
	/// Информация об авторе сообщения
	/// </summary>
	public UserSimpleInfo? Author { get; set; }

	/// <summary>
	/// Информация о затронутом тэге
	/// </summary>
	public TagSimpleInfo? AffectedTag { get; set; }

	/// <summary>
	/// Информация о затронутом источнике
	/// </summary>
	public SourceSimpleInfo? AffectedSource { get; set; }

	/// <summary>
	/// Информация о затронутом блоке
	/// </summary>
	public BlockSimpleInfo? AffectedBlock { get; set; }

	/// <summary>
	/// Информация о затронутой учетной записи
	/// </summary>
	public UserSimpleInfo? AffectedUser { get; set; }

	/// <summary>
	/// Информация о затронутом группе учетных записей
	/// </summary>
	public UserGroupSimpleInfo? AffectedUserGroup { get; set; }

	/// <summary>
	/// Пояснения и дополнительная информация
	/// </summary>
	public string? Details { get; set; }
}
