namespace Datalake.Database.Interfaces;

/// <summary>
/// Модель группы пользователей, защищенная от записи
/// </summary>
public interface IReadOnlyUserGroup
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	Guid Guid { get; }

	/// <summary>
	/// Идентификатор родительской группы
	/// </summary>
	Guid? ParentGuid { get; }

	/// <summary>
	/// Название
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Описание
	/// </summary>
	string? Description { get; }

	/// <summary>
	/// Группа отмечена как удаленная
	/// </summary>
	bool IsDeleted { get; }
} 