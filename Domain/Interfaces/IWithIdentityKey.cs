namespace Datalake.Domain.Interfaces;

/// <summary>
/// Сущность с уникальным числовым ключом
/// </summary>
public interface IWithIdentityKey
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	int Id { get; }
}
