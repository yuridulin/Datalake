using Datalake.InventoryService.Application.Features.Settings.Commands.UpdateSettings;
using Datalake.InventoryService.Application.Features.Settings.Queries.GetSettings;
using Datalake.PrivateApi.Interfaces;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.InventoryService.Api.Controllers;

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

	/// <inheritdoc />
	public override async Task<ActionResult<string>> GetLastUpdateAsync()
	{
		var lastUpdate = dataStore.State.Version;
		return await Task.FromResult(lastUpdate.ToString());
	}

	/// <inheritdoc />


	

	/// <inheritdoc />
	public override async Task<ActionResult> RestartStateAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		await dataStore.RestoreAsync();

		return NoContent();
	}

	/// <inheritdoc />
	public override async Task<ActionResult<Dictionary<Guid, UserAuthInfo>>> GetAccessAsync()
	{
		var user = authenticator.Authenticate(HttpContext);
		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return await Task.FromResult(accessStore.State.GetAll());
	}
}