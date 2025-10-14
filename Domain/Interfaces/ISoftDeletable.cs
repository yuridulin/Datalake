using Datalake.Domain.Exceptions;

namespace Datalake.Domain.Interfaces;

/// <summary>
/// Объект с поддержкой мягкого удаления
/// </summary>
public interface ISoftDeletable
{
	/// <summary>
	/// Флаг, отмечен ли объект как удаленный
	/// </summary>
	bool IsDeleted { get; }

	/// <summary>
	/// Мягкое удаление
	/// </summary>
	/// <exception cref="DomainException">Объект уже удален</exception>
	void MarkAsDeleted();
}
