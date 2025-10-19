using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Interfaces;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице свойств блоков
/// </summary>
public record class BlockProperty : IWithIdentityKey
{
	#region Конструкторы

	private BlockProperty() { }

	#endregion Конструкторы

	#region Свойства

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Идентификатор блока
	/// </summary>
	public int BlockId { get; private set; }

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; private set; } = string.Empty;

	/// <summary>
	/// Тип значения
	/// </summary>
	public TagType Type { get; private set; } = TagType.String;

	/// <summary>
	/// Значение
	/// </summary>
	public string Value { get; private set; } = string.Empty;

	#endregion Свойства

	#region Связи

	/// <summary>
	/// Блок
	/// </summary>
	public Block? Block { get; set; }

	#endregion Связи
}
