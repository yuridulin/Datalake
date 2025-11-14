using Datalake.Domain.Enums;
using Datalake.Domain.Exceptions;
using Datalake.Domain.Interfaces;
using Datalake.Domain.ValueObjects;

namespace Datalake.Domain.Entities;

/// <summary>
/// Источник данных
/// </summary>
public record class Source : IWithIdentityKey, ISoftDeletable
{
	#region Конструкторы

	private Source() { }

	/// <summary>
	/// Новый источник без настроек
	/// </summary>
	/// <param name="type">Тип источника</param>
	/// <returns>Источник даннных</returns>
	public static Source CreateEmpty(SourceType? type)
	{
		var source = new Source()
		{
			Type = type ?? SourceType.Inopc,
		};

		if (type != null)
			source.UpdateType(type.Value);

		return source;
	}

	/// <summary>
	/// Новый внутренний источник
	/// </summary>
	/// <param name="type">Тип источника</param>
	/// <param name="name">Название</param>
	/// <param name="description">Описание</param>
	/// <returns>Источник даннных</returns>
	public static Source CreateAsInternal(SourceType type, string name, string? description)
	{
		return new()
		{
			Type = type,
			Name = name,
			Description = description,
		};
	}

	/// <summary>
	/// Новый источник с настройками
	/// </summary>
	/// <param name="type">Тип источника</param>
	/// <param name="address">Адрес конечной точки</param>
	/// <param name="name">Название</param>
	/// <param name="description">Описание</param>
	/// <returns>Источник даннных</returns>
	public static Source CreateAsExternal(SourceType? type, string? name, string? description, string? address) 
	{
		var source = CreateEmpty(type);

		if (string.IsNullOrEmpty(name))
			return source;

		source.UpdateProperties(name, description, address);
		return source;
	}

	#endregion Конструкторы

	#region Методы

	/// <inheritdoc/>
	public void MarkAsDeleted()
	{
		if (IsDeleted)
			throw new DomainException("Источник данных уже удален");

		IsDeleted = true;
	}
	
	/// <summary>
	/// Изменение типа источника
	/// </summary>
	/// <param name="type">Новый тип источника данных</param>
	/// <exception cref="ArgumentException">Неверный тип</exception>
	public void UpdateType(SourceType type)
	{
		if (InternalSources.Contains(Type))
			throw new ArgumentException("Создавать или изменять встроенные источники данных запрещено");

		Type = type;
	}

	/// <summary>
	/// Изменение настроек
	/// </summary>
	/// <param name="name">Название</param>
	/// <param name="description">Описание</param>
	/// <param name="address">Адрес</param>
	/// <exception cref="DomainException">Ошибки</exception>
	public void UpdateProperties(string name, string? description, string? address)
	{
		if (InternalSources.Contains(Type))
			throw new DomainException(nameof(address), "Изменение встроенных источников данных запрещено");

		if (address == null)
			throw new DomainException(nameof(address), "Для не-встроенного источника данных адрес является обязательным");

		Address = address;
		Name = name;
		Description = description;
	}

	/// <summary>
	/// Встроенные не настраиваемые источники данных
	/// </summary>
	public static IReadOnlyCollection<SourceType> InternalSources { get; } = new SourceType[]
	{
		SourceType.System,
		SourceType.Calculated,
		SourceType.Manual,
		SourceType.Aggregated,
		SourceType.Unset
	};

	#endregion Методы

	#region Свойства

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; private set; } = string.Empty;

	/// <summary>
	/// Описание
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	/// Тип получения данных
	/// </summary>
	public SourceType Type { get; private set; } = SourceType.Inopc;

	/// <summary>
	/// Адрес источника данных
	/// </summary>
	public string? Address { get; private set; }

	/// <summary>
	/// Источник отмечен как удаленный
	/// </summary>
	public bool IsDeleted { get; private set; } = false;

	/// <summary>
	/// Источник отмечен как отключенный (не участвует в сборе данных)
	/// </summary>
	public bool IsDisabled { get; private set; } = false;

	#endregion Свойства

	#region Связи

	/// <summary>
	/// Список тегов, получающих значения
	/// </summary>
	public ICollection<Tag> Tags { get; set; } = [];

	/// <summary>
	/// Список прямых правил доступа на этот источник данных
	/// </summary>
	public ICollection<AccessRule> AccessRules { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита по этому источнику
	/// </summary>
	public ICollection<AuditLog> AuditLogs { get; set; } = [];

	/// <summary>
	/// Рассчитаные для этого источника данных указания фактического доступа
	/// </summary>
	public ICollection<CalculatedAccessRule> CalculatedAccessRules { get; set; } = [];

	#endregion Связи
}
