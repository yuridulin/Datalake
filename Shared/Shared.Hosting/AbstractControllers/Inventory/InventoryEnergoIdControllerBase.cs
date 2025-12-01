using Datalake.Contracts.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Shared.Hosting.AbstractControllers.Inventory;

/// <summary>
/// Учетные записи
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Inventory")]
[Route("api/v1/inventory/energo-id")]
public abstract class InventoryEnergoIdControllerBase : ControllerBase
{
	/// <summary>
	/// Получение списка пользователей, определенных на сервере EnergoId
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список учетных записей EnergoId с отметкой на каждой, за какой учетной записью приложения закреплена</returns>
	[HttpGet]
	public abstract Task<ActionResult<List<UserEnergoIdInfo>>> GetEnergoIdAsync(
		CancellationToken ct = default);

	/// <summary>
	/// Обновление данных из EnergoId
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	[HttpPut]
	public abstract Task<ActionResult<bool>> UpdateEnergoIdAsync(
		CancellationToken ct = default);
}
