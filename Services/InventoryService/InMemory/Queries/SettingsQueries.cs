using Datalake.InventoryService.InMemory.Models;
using Datalake.PublicApi.Models.Settings;

namespace Datalake.Inventory.InMemory.Queries;

/// <summary>
/// Запросы, связанные с настройками
/// </summary>
public static class SettingsQueries
{
	/// <summary>
	/// Запрос информации о текущих настройках
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	public static SettingsInfo SettingsInfo(this DatalakeDataState state)
	{
		var settings = state.Settings;

		return new SettingsInfo
		{
			EnergoIdApi = settings.EnergoIdApi,
			EnergoIdClient = settings.KeycloakClient,
			EnergoIdHost = settings.KeycloakHost,
			InstanceName = settings.InstanceName,
		};
	}
}
