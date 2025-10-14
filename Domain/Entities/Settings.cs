namespace Datalake.Domain.Entities;

/// <summary>
/// Настройки приложения
/// </summary>
public record class Settings
{
	private Settings() { }

	/// <summary>
	/// Создание нового объекта настроек
	/// </summary>
	/// <param name="keycloakHost"></param>
	/// <param name="keycloakClient"></param>
	/// <param name="energoIdApi"></param>
	/// <param name="instanceName"></param>
	public Settings(string keycloakHost, string keycloakClient, string energoIdApi, string instanceName)
	{
		Update(keycloakHost, keycloakClient, energoIdApi, instanceName);
	}

	/// <summary>
	/// Изменение настроек
	/// </summary>
	/// <param name="keycloakHost"></param>
	/// <param name="keycloakClient"></param>
	/// <param name="energoIdApi"></param>
	/// <param name="instanceName"></param>
	public void Update(string keycloakHost, string keycloakClient, string energoIdApi, string instanceName)
	{
		LastUpdate = DateTimeOffset.UtcNow;
		KeycloakHost = keycloakHost;
		KeycloakClient = keycloakClient;
		EnergoIdApi = energoIdApi;
		InstanceName = instanceName;
	}


	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
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
