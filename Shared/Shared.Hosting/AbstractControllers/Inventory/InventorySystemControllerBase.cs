using Datalake.Contracts.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Shared.Hosting.AbstractControllers.Inventory;

/// <summary>
/// Настройки и состояние системы
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Inventory")]
[Route("api/v1/inventory/system")]
public abstract class InventorySystemControllerBase : ControllerBase
{
	/// <summary>
	/// Получение информации о настройках сервера
	/// </summary>
	/// <returns>Информация о настройках</returns>
	[HttpGet("settings")]
	public abstract Task<ActionResult<SettingsInfo>> GetSettingsAsync(
		CancellationToken ct = default);

	/// <summary>
	/// Изменение информации о настройках сервера
	/// </summary>
	/// <param name="newSettings">Новые настройки сервера</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("settings")]
	public abstract Task<ActionResult<bool>> UpdateSettingsAsync(
		[BindRequired][FromBody] SettingsInfo newSettings,
		CancellationToken ct = default);

	/// <summary>
	/// Принудительная перезагрузка состояния БД в кэш
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	[HttpPost("cache")]
	public abstract Task<ActionResult<bool>> RestartStateAsync(
		CancellationToken ct = default);
}
