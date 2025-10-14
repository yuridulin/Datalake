using Datalake.Contracts.Public.Enums;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице записей аудита
/// </summary>
public record class AuditLog
{
	#region Конструкторы

	private AuditLog() { }

	/// <summary>
	/// Сообщение ядра, созданное без пользователя
	/// </summary>
	/// <param name="category">Категория модуля, который создал событие</param>
	/// <param name="message">Сообщение</param>
	/// <param name="details">Расшифровка</param>
	/// <param name="type">Тип</param>
	public AuditLog(LogCategory category, string message, string? details, LogType type = LogType.Information)
	{
		Category = category;
		Type = type;
		Text = message;
		Details = details;
		Date = DateTime.UtcNow;
	}

	/// <summary>
	/// Сообщение о операции с объектом, созданное в результате действий пользователя
	/// </summary>
	/// <param name="authorGuid">Идентификатор пользователя</param>
	/// <param name="message">Сообщение</param>
	/// <param name="details">Расшифровка</param>
	/// <param name="type">Тип</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="userGuid">Идентификатор учетной записи</param>
	/// <param name="userGroupGuid">Идентификатор группы учетных записей</param>
	/// <exception cref="InvalidDataException"></exception>
	public AuditLog(Guid? authorGuid, string message, string? details = null, LogType type = LogType.Success, int? blockId = null, int? tagId = null, int? sourceId = null, Guid? userGuid = null, Guid? userGroupGuid = null)
	{
		Type = type;
		AuthorGuid = authorGuid;
		Text = message;
		Details = details;

		if (blockId != null)
		{
			Category = LogCategory.Blocks;
			AffectedBlockId = blockId;
			RefId = blockId.ToString();
		}
		else if (tagId != null)
		{
			Category = LogCategory.Tag;
			AffectedTagId = tagId;
			RefId = tagId.ToString();
		}
		else if (sourceId != null)
		{
			Category = LogCategory.Source;
			AffectedSourceId = sourceId;
			RefId = sourceId.ToString();
		}
		else if (userGuid != null)
		{
			Category = LogCategory.Users;
			AffectedUserGuid = userGuid;
			RefId = userGuid.ToString();
		}
		else if (userGroupGuid != null)
		{
			Category = LogCategory.UserGroups;
			AffectedUserGroupGuid = userGroupGuid;
			RefId = userGroupGuid.ToString();
		}
		else
			throw new InvalidDataException("Ни один из идентификаторов не указан");
	}

	#endregion Конструкторы

	#region Поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public long Id { get; private set; }

	/// <summary>
	/// Дата записи
	/// </summary>
	public DateTime Date { get; private set; }

	/// <summary>
	/// Категория
	/// </summary>
	public LogCategory Category { get; private set; } = LogCategory.Api;

	/// <summary>
	/// Идентификатор затронутого источника данных
	/// </summary>
	public int? AffectedSourceId { get; private set; }

	/// <summary>
	/// Идентификатор затронутого тега
	/// </summary>
	public int? AffectedTagId { get; private set; }

	/// <summary>
	/// Идентификатор затронутого блока
	/// </summary>
	public int? AffectedBlockId { get; private set; }

	/// <summary>
	/// Идентификатор затронутой учетной записи
	/// </summary>
	public Guid? AffectedUserGuid { get; private set; }

	/// <summary>
	/// Идентификатор затронутой группы учетных записей
	/// </summary>
	public Guid? AffectedUserGroupGuid { get; private set; }

	/// <summary>
	/// Идентификатор затронутого правила доступа
	/// </summary>
	public int? AffectedAccessRightsId { get; private set; }

	/// <summary>
	/// Идентификатор связанного объекта
	/// </summary>
	public string? RefId { get; private set; }

	/// <summary>
	/// Идентификатор пользователя, совершившего записанное действие
	/// </summary>
	public Guid? AuthorGuid { get; private set; }

	/// <summary>
	/// Тип
	/// </summary>
	public LogType Type { get; private set; } = LogType.Information;

	/// <summary>
	/// Сообщение о событии
	/// </summary>
	public string Text { get; private set; } = string.Empty;

	/// <summary>
	/// Пояснения и дополнительная информация
	/// </summary>
	public string? Details { get; private set; }

	#endregion Поля в БД

	#region Связи

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
	public AccessRule? AffectedAccessRights { get; set; }

	#endregion Связи
}
