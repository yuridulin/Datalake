using Datalake.PublicApi.Enums;

namespace Datalake.Database.Interfaces;

/// <summary>
/// Модель прав доступа, защищенная от записи
/// </summary>
public interface IReadOnlyAccessRights
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	int Id { get; }

	/// <summary>
	/// Идентификатор пользователя, на которого выдано правило
	/// </summary>
	Guid? UserGuid { get; }

	/// <summary>
	/// Идентификатор группы, на которую выдано правило
	/// </summary>
	Guid? UserGroupGuid { get; }

	/// <summary>
	/// Это правило глобальное для всего приложения?
	/// </summary>
	bool IsGlobal { get; }

	/// <summary>
	/// Идентификатор тега, на который действует правило
	/// </summary>
	int? TagId { get; }

	/// <summary>
	/// Идентификатор источника, на который действует правило
	/// </summary>
	int? SourceId { get; }

	/// <summary>
	/// Идентификатор блока, на который действует правило
	/// </summary>
	int? BlockId { get; }

	/// <summary>
	/// Выданный уровень доступа
	/// </summary>
	AccessType AccessType { get; }
} 