using Datalake.InventoryService.Domain.Constants;
using Datalake.InventoryService.Domain.Interfaces;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Domain.Entities;

/// <summary>
/// Запись в таблице источников
/// </summary>
public record class SourceEntity : IWithIdentityKey, ISoftDeletable
{
	private SourceEntity() { }

	/// <summary>
	/// Новый источник без настроек
	/// </summary>
	/// <param name="type">Тип источника</param>
	public SourceEntity(SourceType type)
	{
		if (Lists.CustomSources.Contains(Type))
			throw new ArgumentException("Запрещено создавать встроенные источники данных");

		Type = type;
	}

	/// <summary>
	/// Новый источник с настройками настроек
	/// </summary>
	/// <param name="type">Тип источника</param>
	/// <param name="address">Адрес конечной точки</param>
	/// <param name="name">Название</param>
	/// <param name="description">Описание</param>
	public SourceEntity(SourceType type, string? name, string? address, string? description) : this(type)
	{
		if (string.IsNullOrEmpty(name))
			return;

		if (address == null)
			throw new ArgumentNullException(nameof(address), "Адрес источника данных является обязательным");

		Address = address;
		Name = name;
		Description = description;
	}

	public void MarkAsDeleted()
	{
		IsDeleted = true;
	}

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
	public ICollection<TagEntity> Tags { get; set; } = [];

	/// <summary>
	/// Список правил доступа, действующих на источник
	/// </summary>
	public ICollection<AccessRuleEntity> AccessRightsList { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<AuditEntity> Logs { get; set; } = null!;
}
