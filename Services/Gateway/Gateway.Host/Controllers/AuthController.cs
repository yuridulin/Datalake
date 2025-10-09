using Datalake.Gateway.Api.Models.Auth;
using Datalake.Gateway.Application.Features.Commands.CloseSession;
using Datalake.Gateway.Application.Features.Commands.OpenEnergoIdSession;
using Datalake.Gateway.Application.Features.Commands.OpenLocalSession;
using Datalake.Gateway.Application.Features.Queries.GetCurrentSessionWithAccess;
using Datalake.Gateway.Application.Models;
using Datalake.Gateway.Host.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.Controllers;

/// <summary>
/// Управление сессиями, логин/логаут
/// </summary>
[ApiController]
[Route("api/v1/sessions")]
public class AuthController(ISessionTokenExtractor tokenExtractor) : ControllerBase
{
	/// <summary>
	/// <see cref="HttpMethod.Post" />: Аутентификация локального пользователя по связке "имя для входа/пароль"
	/// </summary>
	/// <param name="request">Данные для входа</param>
	/// <returns>Данные о учетной записи</returns>
	[HttpPost("local")]
	public async Task<ActionResult<SessionInfo>> AuthenticateLocalAsync(
		[FromServices] IOpenLocalSessionHandler openHandler,
		[FromServices] IGetCurrentSessionWithAccessHandler getHandler,
		[BindRequired, FromBody] AuthLoginPassRequest request,
		CancellationToken ct = default)
	{
		var sessionToken = await openHandler.HandleAsync(new() { Login = request.Login, PasswordString = request.Password }, ct);
		var sessionInfo = await getHandler.HandleAsync(new() { Token = sessionToken }, ct);

		return Ok(sessionInfo);
	}

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Аутентификация пользователя, прошедшего проверку на сервере EnergoId
	/// </summary>
	/// <param name="request">Данные пользователя Keycloak</param>
	/// <returns>Данные о учетной записи</returns>
	[HttpPost("energo-id")]
	public async Task<ActionResult<SessionInfo>> AuthenticateEnergoIdUserAsync(
		[FromServices] IOpenEnergoIdSessionHandler openHandler,
		[FromServices] IGetCurrentSessionWithAccessHandler getHandler,
		[BindRequired, FromBody] AuthEnergoIdRequest request,
		CancellationToken ct = default)
	{
		var sessionToken = await openHandler.HandleAsync(new() { Guid = request.EnergoIdGuid }, ct);
		var sessionInfo = await getHandler.HandleAsync(new() { Token = sessionToken }, ct);

		return Ok(sessionInfo);
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение информации о учетной записи на основе текущей сессии
	/// </summary>
	/// <returns>Данные о учетной записи</returns>
	[HttpGet("identify")]
	public async Task<ActionResult<SessionInfo?>> IdentifyAsync(
		[FromServices] IGetCurrentSessionWithAccessHandler handler,
		CancellationToken ct = default)
	{
		var token = tokenExtractor.ExtractToken(HttpContext);

		var sessionInfo = await handler.HandleAsync(new() { Token = token }, ct);

		return Ok(sessionInfo);
	}

	/// <summary>
	/// <see cref="HttpMethod.Delete"/>: Закрытие уканной сессии пользователя
	/// </summary>
	[HttpDelete]
	public async Task<ActionResult> LogoutAsync(
		[FromServices] ICloseSessionHandler handler,
		CancellationToken ct = default)
	{
		var token = tokenExtractor.ExtractToken(HttpContext);

		await handler.HandleAsync(new() { Token = token }, ct);

		return NoContent();
	}
}
