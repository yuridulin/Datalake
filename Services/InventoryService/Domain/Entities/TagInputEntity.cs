using Datalake.InventoryService.Domain.Interfaces;

namespace Datalake.InventoryService.Domain.Entities;

/// <summary>
/// Запись в таблице связей тега с другими тегами для вычисления значений
/// </summary>
public record class TagInputEntity : IWithIdentityKey
{
	private TagInputEntity() { }

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Идентификатор результиющего тега
	/// </summary>
	public int TagId { get; private set; }

	/// <summary>
	/// Идентификатор входного тега
	/// </summary>
	public int? InputTagId { get; private set; }

	/// <summary>
	/// Идентификатор блока с входным тегом
	/// </summary>
	public int? InputBlockId { get; private set; }

	/// <summary>
	/// Имя переменной в формуле
	/// </summary>
	public string VariableName { get; private set; } = string.Empty;

	// связи

	/// <summary>
	/// Результирующий тег
	/// </summary>
	public TagEntity Tag { get; set; } = null!;

	/// <summary>
	/// Входной тег
	/// </summary>
	public TagEntity? InputTag { get; set; }
}
