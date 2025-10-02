using Datalake.Inventory.Api.Models.Settings;
using Datalake.Inventory.Application.Features.Cache.Commands.ReloadCache;
using Datalake.Inventory.Application.Features.Settings.Commands.UpdateSettings;
using Datalake.Inventory.Application.Features.Settings.Queries.GetSettings;
using Datalake.Shared.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.App.Controllers;

/// <summary>
/// Настройки и состояние системы
/// </summary>
[ApiController]
[Route("api/v1/system")]
public class SystemController(IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение информации о настройках сервера
	/// </summary>
	/// <returns>Информация о настройках</returns>
	[HttpGet("settings")]
	public async Task<ActionResult<SettingsInfo>> GetSettingsAsync(
		[FromServices] IGetSettingsHandler handler,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new(user), ct);

		return data;
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение информации о настройках сервера
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="newSettings">Новые настройки сервера</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("settings")]
	public async Task<ActionResult> UpdateSettingsAsync(
		[FromServices] IUpdateSettingsHandler handler,
		[BindRequired][FromBody] SettingsInfo newSettings,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new(
			user,
			KeycloakClient: newSettings.EnergoIdClient,
			KeycloakHost: newSettings.EnergoIdHost,
			EnergoIdApi: newSettings.EnergoIdApi,
			InstanceName: newSettings.InstanceName), ct);

		return NoContent();
	}

	/// <summary>
	/// <see cref="HttpMethod.Post" /> Принудительная перезагрузка состояния БД в кэш
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPost("cache")]
	public async Task<ActionResult> RestartStateAsync(
		[FromServices] IReloadCacheHandler handler,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new() { User  = user }, ct);

		return NoContent();
	}
}