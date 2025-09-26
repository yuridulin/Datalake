using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись в таблице источников
/// </summary>
public record class Source
{
	private Source() { }

	private static readonly SourceType[] notAllowedTypes = [
		SourceType.NotSet,
		SourceType.Aggregated,
		SourceType.Calculated,
		SourceType.Manual,
		SourceType.System,
	];

	/// <summary>
	/// Новый источник без настроек
	/// </summary>
	/// <param name="type">Тип источника</param>
	public Source(SourceType type)
	{
		if (notAllowedTypes.Contains(Type))
			throw new ArgumentException("Запрещено создавать системные источники данных");

		Type = type;
	}

	/// <summary>
	/// Новый источник с настройками настроек
	/// </summary>
	/// <param name="type">Тип источника</param>
	/// <param name="address">Адрес конечной точки</param>
	/// <param name="name">Название</param>
	/// <param name="description">Описание</param>
	public Source(SourceType type, string? address, string name, string? description) : this(type)
	{
		if (address == null)
			throw new ArgumentNullException(nameof(address), "Адрес источника данных является обязательным");

		Address = address;
		Name = name;
		Description = description;
	}

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Описание
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Тип получения данных
	/// </summary>
	public SourceType Type { get; set; } = SourceType.Inopc;

	/// <summary>
	/// Адрес источника данных
	/// </summary>
	public string? Address { get; set; }

	/// <summary>
	/// Источник отмечен как удаленный
	/// </summary>
	public bool IsDeleted { get; set; } = false;

	/// <summary>
	/// Источник отмечен как отключенный (не участвует в сборе данных)
	/// </summary>
	public bool IsDisabled { get; set; } = false;

	// связи

	/// <summary>
	/// Список тегов, получающих значения
	/// </summary>
	public ICollection<Tag> Tags { get; set; } = [];

	/// <summary>
	/// Список правил доступа, действующих на источник
	/// </summary>
	public ICollection<AccessRights> AccessRightsList { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<Log> Logs { get; set; } = null!;
}
