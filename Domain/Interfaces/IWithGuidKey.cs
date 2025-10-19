namespace Datalake.Domain.Interfaces;

/// <summary>
/// Сущность с уникальным ключом типа GUID
/// </summary>
public interface IWithGuidKey
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	Guid Guid { get; }
}
