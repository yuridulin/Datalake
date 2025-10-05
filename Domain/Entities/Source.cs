using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Exceptions;
using Datalake.Domain.Interfaces;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице источников
/// </summary>
public record class Source : IWithIdentityKey, ISoftDeletable
{
	private Source() { }

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
	/// Новый источник без настроек
	/// </summary>
	/// <param name="type">Тип источника</param>
	public Source(SourceType? type)
	{
		type ??= SourceType.Inopc;
		UpdateType(type.Value);
	}

	/// <summary>
	/// Новый источник с настройками настроек
	/// </summary>
	/// <param name="type">Тип источника</param>
	/// <param name="address">Адрес конечной точки</param>
	/// <param name="name">Название</param>
	/// <param name="description">Описание</param>
	public Source(SourceType? type, string? name, string? description, string? address) : this(type)
	{
		if (string.IsNullOrEmpty(name))
			return;

		UpdateProperties(name, description, address);
	}

	public void MarkAsDeleted()
	{
		if (IsDeleted)
			throw new DomainException("Источник данных уже удален");

		IsDeleted = true;
	}

	public void UpdateType(SourceType type)
	{
		if (CustomSources.Contains(Type))
			throw new ArgumentException("Создавать или изменять встроенные источники данных запрещено");

		Type = type;
	}

	public void UpdateProperties(string name, string? description, string? address)
	{
		if (CustomSources.Contains(Type))
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
	public static IReadOnlyCollection<SourceType> CustomSources { get; } = new SourceType[]
	{
		SourceType.System,
		SourceType.Calculated,
		SourceType.Manual,
		SourceType.Aggregated,
		SourceType.Unset
	};

	// поля в БД

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

	// связи

	/// <summary>
	/// Список тегов, получающих значения
	/// </summary>
	public ICollection<Tag> Tags { get; set; } = [];

	/// <summary>
	/// Список правил доступа, действующих на источник
	/// </summary>
	public ICollection<AccessRights> AccessRules { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<Log> Logs { get; set; } = null!;
}
