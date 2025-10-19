using Datalake.Domain.Exceptions;
using Datalake.Domain.Interfaces;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице связей тега с другими тегами для вычисления значений
/// </summary>
public record class TagInput : IWithIdentityKey
{
	#region Конструкторы

	private TagInput() { }

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="inputTagId">Идентификатор входного тега</param>
	/// <param name="inputBlockId">Идентификатор входного блока</param>
	/// <param name="variable">Имя переменной</param>
	/// <exception cref="DomainException"></exception>
	public TagInput(int tagId, int inputTagId, int? inputBlockId, string variable)
	{
		if (string.IsNullOrWhiteSpace(variable))
			throw new DomainException($"Входной тег {inputTagId} для {tagId}: имя переменной не указано");

		TagId = tagId;
		InputTagId = inputTagId;
		InputBlockId = inputBlockId;
		VariableName = variable;
	}

	#endregion Конструкторы

	#region Свойства

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

	#endregion Свойства

	#region Связи

	/// <summary>
	/// Результирующий тег
	/// </summary>
	public Tag Tag { get; set; } = null!;

	/// <summary>
	/// Входной тег
	/// </summary>
	public Tag? InputTag { get; set; }

	#endregion Связи
}
