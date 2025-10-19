using Datalake.Domain.Interfaces;

namespace Datalake.Domain.Entities;

/// <summary>
/// Порог значения
/// </summary>
public record class TagThreshold : IWithIdentityKey
{
	#region Конструкторы

	private TagThreshold() { }

	/// <summary>
	/// Создание порога для тега
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="input">Входное значение</param>
	/// <param name="output">Результирующее значение</param>
	public TagThreshold(int tagId, float input, float output)
	{
		TagId = tagId;
		InputValue = input;
		OutputValue = output;
	}

	#endregion Конструкторы

	#region Свойства

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Идентификатор тега
	/// </summary>
	public int TagId { get; private set; }

	/// <summary>
	/// Входное значение
	/// </summary>
	public float InputValue { get; private set; }

	/// <summary>
	/// Результирующее значение
	/// </summary>
	public float OutputValue { get; private set; }

	#endregion Свойства

	#region Связи

	/// <summary>
	/// Тег, использующий эту уставку
	/// </summary>
	public Tag Tag { get; set; } = null!;

	#endregion Связи
}
