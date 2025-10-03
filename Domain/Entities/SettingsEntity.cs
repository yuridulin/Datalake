using Datalake.Contracts.Public.Extensions;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице настроек приложения
/// </summary>
public record class SettingsEntity
{
	private SettingsEntity() { }

	public SettingsEntity(string keycloakHost, string keycloakClient, string energoIdApi, string instanceName)
	{
		Update(keycloakHost, keycloakClient, energoIdApi, instanceName);
	}

	public void Update(string keycloakHost, string keycloakClient, string energoIdApi, string instanceName)
	{
		LastUpdate = DateTimeExtension.GetCurrentDateTime();
		KeycloakHost = keycloakHost;
		KeycloakClient = keycloakClient;
		EnergoIdApi = energoIdApi;
		InstanceName = instanceName;
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
