namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись в таблице настроек приложения
/// </summary>
public record class Settings
{
	private Settings() { }

	public Settings(DateTime lastUpdate)
	{
		LastUpdate = lastUpdate;
	}

	// поля в БД

	/// <summary>
	/// Время последнего обновления структуры базы данных
	/// </summary>
	public DateTime LastUpdate { get; set; }

	/// <summary>
	/// Сетевое расположение сервера Keycloak EnergoId
	/// </summary>
	public string KeycloakHost { get; set; } = string.Empty;

	/// <summary>
	/// Название клиента в Keycloak EnergoId
	/// </summary>
	public string KeycloakClient { get; set; } = "datalake";

	/// <summary>
	/// Сетевое расположение сервера API EnergoId
	/// </summary>
	public string EnergoIdApi { get; set; } = string.Empty;

	/// <summary>
	/// Название текущей базы данных
	/// </summary>
	public string InstanceName { get; set; } = string.Empty;
}
