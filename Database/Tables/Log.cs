using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице записей аудита
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public record class Log
{
	const string TableName = "Logs";

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long Id { get; set; }

	/// <summary>
	/// Дата записи
	/// </summary>
	[Column, NotNull]
	public DateTime Date { get; set; } = DateFormats.GetCurrentDateTime();

	/// <summary>
	/// Категория
	/// </summary>
	[Column, NotNull]
	public LogCategory Category { get; set; } = LogCategory.Api;

	/// <summary>
	/// Идентификатор затронутого источника данных
	/// </summary>
	[Column]
	public int? AffectedSourceId { get; set; }

	/// <summary>
	/// Идентификатор затронутого тега
	/// </summary>
	[Column]
	public int? AffectedTagId { get; set; }

	/// <summary>
	/// Идентификатор затронутого блока
	/// </summary>
	[Column]
	public int? AffectedBlockId { get; set; }

	/// <summary>
	/// Идентификатор затронутой учетной записи
	/// </summary>
	[Column]
	public Guid? AffectedUserGuid { get; set; }

	/// <summary>
	/// Идентификатор затронутой группы учетных записей
	/// </summary>
	[Column]
	public Guid? AffectedUserGroupGuid { get; set; }

	/// <summary>
	/// Идентификатор затронутого правила доступа
	/// </summary>
	[Column]
	public int? AffectedAccessRightsId { get; set; }

	/// <summary>
	/// Идентификатор связанного объекта
	/// </summary>
	[Column]
	public string? RefId { get; set; }

	/// <summary>
	/// Идентификатор пользователя, совершившего записанное действие
	/// </summary>
	[Column]
	public Guid? AuthorGuid { get; set; }

	/// <summary>
	/// Тип
	/// </summary>
	[Column, NotNull]
	public LogType Type { get; set; } = LogType.Information;

	/// <summary>
	/// Сообщение о событии
	/// </summary>
	[Column, NotNull]
	public string Text { get; set; } = string.Empty;

	/// <summary>
	/// Пояснения и дополнительная информация
	/// </summary>
	[Column]
	public string? Details { get; set; }

	// связи

	/// <summary>
	/// Пользователь, совершивший записанное действие
	/// </summary>
	public User? Author { get; set; }

	/// <summary>
	/// Затронутый источник данных
	/// </summary>
	public Source? AffectedSource { get; set; }

	/// <summary>
	/// Затронутый тег
	/// </summary>
	public Tag? AffectedTag { get; set; }

	/// <summary>
	/// Затронутый блок
	/// </summary>
	public Block? AffectedBlock { get; set; }

	/// <summary>
	/// Затронутая учетная запись
	/// </summary>
	public User? AffectedUser { get; set; }

	/// <summary>
	/// Затронутая группа учетных записей
	/// </summary>
	public UserGroup? AffectedUserGroup { get; set; }

	/// <summary>
	/// Затронутое правило доступа
	/// </summary>
	public AccessRights? AffectedAccessRights { get; set; }
}
