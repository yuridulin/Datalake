using Datalake.Contracts.Public.Models.Settings;
using Datalake.Inventory.Application.Features.Cache.Commands.ReloadCache;
using Datalake.Inventory.Application.Features.Settings.Commands.UpdateSettings;
using Datalake.Inventory.Application.Features.Settings.Queries.GetSettings;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

/// <summary>
/// Настройки и состояние системы
/// </summary>
[ApiController]
[Route("api/system")]
public class SystemController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// Получение информации о настройках сервера
	/// </summary>
	/// <returns>Информация о настройках</returns>
	[HttpGet("settings")]
	public async Task<ActionResult<SettingsInfo>> GetSettingsAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetSettingsHandler>();
		var data = await handler.HandleAsync(new(user), ct);

		return data;
	}

	/// <summary>
	/// Изменение информации о настройках сервера
	/// </summary>
	/// <param name="newSettings">Новые настройки сервера</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("settings")]
	public async Task<ActionResult> UpdateSettingsAsync(
		[BindRequired][FromBody] SettingsInfo newSettings,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateSettingsHandler>();
		await handler.HandleAsync(new(
			user,
			KeycloakClient: newSettings.EnergoIdClient,
			KeycloakHost: newSettings.EnergoIdHost,
			EnergoIdApi: newSettings.EnergoIdApi,
			InstanceName: newSettings.InstanceName), ct);

		return NoContent();
	}

	/// <summary>
	/// Принудительная перезагрузка состояния БД в кэш
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	[HttpPost("cache")]
	public async Task<ActionResult> RestartStateAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IReloadCacheHandler>();
		await handler.HandleAsync(new() { User = user }, ct);

		return NoContent();
	}
}