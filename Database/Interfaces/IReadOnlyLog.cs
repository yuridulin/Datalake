using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;

namespace Datalake.Database.Interfaces;

/// <summary>
/// Модель лога, защищенная от записи
/// </summary>
public interface IReadOnlyLog
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	long Id { get; }

	/// <summary>
	/// Дата записи
	/// </summary>
	DateTime Date { get; }

	/// <summary>
	/// Категория
	/// </summary>
	LogCategory Category { get; }

	/// <summary>
	/// Идентификатор затронутого источника данных
	/// </summary>
	int? AffectedSourceId { get; }

	/// <summary>
	/// Идентификатор затронутого тега
	/// </summary>
	int? AffectedTagId { get; }

	/// <summary>
	/// Идентификатор затронутого блока
	/// </summary>
	int? AffectedBlockId { get; }

	/// <summary>
	/// Идентификатор затронутой учетной записи
	/// </summary>
	Guid? AffectedUserGuid { get; }

	/// <summary>
	/// Идентификатор затронутой группы учетных записей
	/// </summary>
	Guid? AffectedUserGroupGuid { get; }

	/// <summary>
	/// Идентификатор затронутого правила доступа
	/// </summary>
	int? AffectedAccessRightsId { get; }

	/// <summary>
	/// Идентификатор связанного объекта
	/// </summary>
	string? RefId { get; }

	/// <summary>
	/// Идентификатор пользователя, совершившего записанное действие
	/// </summary>
	Guid? AuthorGuid { get; }

	/// <summary>
	/// Тип
	/// </summary>
	LogType Type { get; }

	/// <summary>
	/// Сообщение о событии
	/// </summary>
	string Text { get; }

	/// <summary>
	/// Пояснения и дополнительная информация
	/// </summary>
	string? Details { get; }
} 