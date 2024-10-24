namespace Datalake.Database.Models.AccessRights;

/// <summary>
/// Измененное разрешение, которое нужно обновить в БД
/// </summary>
public class AccessRightsApplyRequest
{
	/// <summary>
	/// Идентификатор пользователя, которому выдается разрешение
	/// </summary>
	public Guid? UserGuid { get; set; }

	/// <summary>
	/// Идентификатор группы пользователей, которой выдается разрешение
	/// </summary>
	public Guid? UserGroupGuid { get; set; }

	/// <summary>
	/// Идентификатор источника, на который выдается разрешение
	/// </summary>
	public int? SourceId { get; set; }

	/// <summary>
	/// Идентификатор блока, на который выдается разрешение
	/// </summary>
	public int? BlockId { get; set; }

	/// <summary>
	/// Идентификатор тега, на который выдается разрешение
	/// </summary>
	public int? TagId { get; set; }

	/// <summary>
	/// Список прав доступа
	/// </summary>
	public required AccessRightsIdInfo[] Rights { get; set; }
}
