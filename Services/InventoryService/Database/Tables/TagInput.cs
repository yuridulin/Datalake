namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись в таблице связей тега с другими тегами для вычисления значений
/// </summary>
public record class TagInput
{
	private TagInput() { }

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Идентификатор результиющего тега
	/// </summary>
	public int TagId { get; set; }

	/// <summary>
	/// Идентификатор входного тега
	/// </summary>
	public int? InputTagId { get; set; }

	/// <summary>
	/// Идентификатор блока с входным тегом
	/// </summary>
	public int? InputBlockId { get; set; }

	/// <summary>
	/// Имя переменной в формуле
	/// </summary>
	public string VariableName { get; set; } = string.Empty;

	// связи

	/// <summary>
	/// Результирующий тег
	/// </summary>
	public Tag Tag { get; set; } = null!;

	/// <summary>
	/// Входной тег
	/// </summary>
	public Tag? InputTag { get; set; }
}
