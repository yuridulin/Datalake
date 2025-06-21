namespace Datalake.Database.Interfaces;

/// <summary>
/// Модель настроек, защищенная от записи
/// </summary>
public interface IReadOnlySettings
{
	/// <summary>
	/// Время последнего обновления структуры базы данных
	/// </summary>
	DateTime LastUpdate { get; }

	/// <summary>
	/// Сетевое расположение сервера Keycloak EnergoId
	/// </summary>
	string KeycloakHost { get; }

	/// <summary>
	/// Название клиента в Keycloak EnergoId
	/// </summary>
	string KeycloakClient { get; }

	/// <summary>
	/// Сетевое расположение сервера API EnergoId
	/// </summary>
	string EnergoIdApi { get; }

	/// <summary>
	/// Название текущей базы данных
	/// </summary>
	string InstanceName { get; }
} 