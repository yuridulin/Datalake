namespace Datalake.Database.Interfaces;

/// <summary>
/// Модель блока, защищенная от записи
/// </summary>
public interface IReadOnlyBlock
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	int Id { get; }

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	Guid GlobalId { get; }

	/// <summary>
	/// Идентификатор родительского блока
	/// </summary>
	int? ParentId { get; }

	/// <summary>
	/// Название
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Описание
	/// </summary>
	string? Description { get; }

	/// <summary>
	/// Блок отмечен как удаленный
	/// </summary>
	bool IsDeleted { get; }
}