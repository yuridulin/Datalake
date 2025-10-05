namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице настроек приложения
/// </summary>
public record class Settings
{
	private Settings() { }

	public Settings(string keycloakHost, string keycloakClient, string energoIdApi, string instanceName)
	{
		Update(keycloakHost, keycloakClient, energoIdApi, instanceName);
	}

	public void Update(string keycloakHost, string keycloakClient, string energoIdApi, string instanceName)
	{
		LastUpdate = DateTimeOffset.UtcNow;
		KeycloakHost = keycloakHost;
		KeycloakClient = keycloakClient;
		EnergoIdApi = energoIdApi;
		InstanceName = instanceName;
	}


	// поля в БД

	public int Id { get; private set; }

	/// <summary>
	/// Время последнего обновления структуры базы данных
	/// </summary>
	public DateTimeOffset LastUpdate { get; private set; }

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
