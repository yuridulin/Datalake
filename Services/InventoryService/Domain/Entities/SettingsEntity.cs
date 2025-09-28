namespace Datalake.InventoryService.Domain.Entities;

/// <summary>
/// Запись в таблице настроек приложения
/// </summary>
public record class SettingsEntity
{
	private SettingsEntity() { }

	public SettingsEntity(DateTime lastUpdate)
	{
		LastUpdate = lastUpdate;
	}

	// поля в БД

	/// <summary>
	/// Время последнего обновления структуры базы данных
	/// </summary>
	public DateTime LastUpdate { get; private set; }

	/// <summary>
	/// Сетевое расположение сервера Keycloak EnergoId
	/// </summary>
	public string KeycloakHost { get; private set; } = string.Empty;

	/// <summary>
	/// Название клиента в Keycloak EnergoId
	/// </summary>
	public string KeycloakClient { get; private set; } = "datalake";

	/// <summary>
	/// Сетевое расположение сервера API EnergoId
	/// </summary>
	public string EnergoIdApi { get; private set; } = string.Empty;

	/// <summary>
	/// Название текущей базы данных
	/// </summary>
	public string InstanceName { get; private set; } = string.Empty;
}
