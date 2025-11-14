using Datalake.Gateway.Application.Features.Commands.CloseSession;
using Datalake.Gateway.Application.Features.Commands.OpenEnergoIdSession;
using Datalake.Gateway.Application.Features.Commands.OpenLocalSession;
using Datalake.Gateway.Application.Features.Queries.GetCurrentSessionWithAccess;
using Datalake.Gateway.Application.Models;
using Datalake.Gateway.Application.Models.Auth;
using Datalake.Gateway.Host.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.Controllers;

/// <summary>
/// Управление сессиями, логин/логаут
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Gateway")]
[Route("api/v1/sessions")]
public class AuthController(
	IServiceProvider serviceProvider,
	ISessionTokenExtractor tokenExtractor) : ControllerBase
{
	/// <summary>
	/// Аутентификация локального пользователя по связке "имя для входа/пароль"
	/// </summary>
	/// <param name="request">Данные для входа</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Данные о учетной записи</returns>
	[HttpPost("local")]
	public async Task<ActionResult<SessionInfo>> AuthenticateLocalAsync(
		[BindRequired, FromBody] AuthLoginPassRequest request,
		CancellationToken ct = default)
	{
		var openHandler = serviceProvider.GetRequiredService<IOpenLocalSessionHandler>();
		var getHandler = serviceProvider.GetRequiredService<IGetCurrentSessionWithAccessHandler>();

		var sessionToken = await openHandler.HandleAsync(new() { Login = request.Login, PasswordString = request.Password }, ct);
		var sessionInfo = await getHandler.HandleAsync(new() { Token = sessionToken }, ct);

		return Ok(sessionInfo);
	}

	/// <summary>
	/// Аутентификация пользователя, прошедшего проверку на сервере EnergoId
	/// </summary>
	/// <param name="request">Данные пользователя Keycloak</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Данные о учетной записи</returns>
	[HttpPost("energo-id")]
	public async Task<ActionResult<SessionInfo>> AuthenticateEnergoIdUserAsync(
		[BindRequired, FromBody] AuthEnergoIdRequest request,
		CancellationToken ct = default)
	{
		var openHandler = serviceProvider.GetRequiredService<IOpenEnergoIdSessionHandler>();
		var getHandler = serviceProvider.GetRequiredService<IGetCurrentSessionWithAccessHandler>();

		var sessionToken = await openHandler.HandleAsync(new() { Guid = request.EnergoIdGuid }, ct);
		var sessionInfo = await getHandler.HandleAsync(new() { Token = sessionToken }, ct);

		return Ok(sessionInfo);
	}

	/// <summary>
	/// Получение информации о учетной записи на основе текущей сессии
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Данные о учетной записи</returns>
	[HttpGet("identify")]
	public async Task<ActionResult<SessionInfo?>> IdentifyAsync(
		CancellationToken ct = default)
	{
		var token = tokenExtractor.ExtractToken(HttpContext);

		var handler = serviceProvider.GetRequiredService<IGetCurrentSessionWithAccessHandler>();

		var sessionInfo = await handler.HandleAsync(new() { Token = token }, ct);

		return Ok(sessionInfo);
	}

	/// <summary>
	/// <see cref="HttpMethod.Delete"/>: Закрытие уканной сессии пользователя
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete]
	public async Task<ActionResult> LogoutAsync(
		CancellationToken ct = default)
	{
		var token = tokenExtractor.ExtractToken(HttpContext);

		var handler = serviceProvider.GetRequiredService<ICloseSessionHandler>();

		await handler.HandleAsync(new() { Token = token }, ct);

		return NoContent();
	}
}
