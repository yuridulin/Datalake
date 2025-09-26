using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись в таблице записей аудита
/// </summary>
public record class Log
{
	private Log() { }

	public Log(LogCategory category, LogType type, Guid? authorGuid, string message, string? details, int? blockId = null, int? tagId = null, int? sourceId = null, Guid? userGuid = null, Guid? groupGuid = null)
	{
		Category = category;
		Type = type;
		AuthorGuid = authorGuid;
		Text = message;
		Details = details;

		if (blockId != null)
		{
			AffectedBlockId = blockId;
			RefId = blockId.ToString();
		}
		else if (tagId != null)
		{
			AffectedTagId = tagId;
			RefId = tagId.ToString();
		}
		else if (sourceId != null)
		{
			AffectedSourceId = sourceId;
			RefId = sourceId.ToString();
		}
		else if (userGuid != null)
		{
			AffectedUserGuid = userGuid;
			RefId = userGuid.ToString();
		}
		else if (groupGuid != null)
		{
			AffectedUserGroupGuid = groupGuid;
			RefId = groupGuid.ToString();
		}
	}

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public long Id { get; set; }

	/// <summary>
	/// Дата записи
	/// </summary>
	public DateTime Date { get; set; } = DateFormats.GetCurrentDateTime();

	/// <summary>
	/// Категория
	/// </summary>
	public LogCategory Category { get; set; } = LogCategory.Api;

	/// <summary>
	/// Идентификатор затронутого источника данных
	/// </summary>
	public int? AffectedSourceId { get; set; }

	/// <summary>
	/// Идентификатор затронутого тега
	/// </summary>
	public int? AffectedTagId { get; set; }

	/// <summary>
	/// Идентификатор затронутого блока
	/// </summary>
	public int? AffectedBlockId { get; set; }

	/// <summary>
	/// Идентификатор затронутой учетной записи
	/// </summary>
	public Guid? AffectedUserGuid { get; set; }

	/// <summary>
	/// Идентификатор затронутой группы учетных записей
	/// </summary>
	public Guid? AffectedUserGroupGuid { get; set; }

	/// <summary>
	/// Идентификатор затронутого правила доступа
	/// </summary>
	public int? AffectedAccessRightsId { get; set; }

	/// <summary>
	/// Идентификатор связанного объекта
	/// </summary>
	public string? RefId { get; set; }

	/// <summary>
	/// Идентификатор пользователя, совершившего записанное действие
	/// </summary>
	public Guid? AuthorGuid { get; set; }

	/// <summary>
	/// Тип
	/// </summary>
	public LogType Type { get; set; } = LogType.Information;

	/// <summary>
	/// Сообщение о событии
	/// </summary>
	public string Text { get; set; } = string.Empty;

	/// <summary>
	/// Пояснения и дополнительная информация
	/// </summary>
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
