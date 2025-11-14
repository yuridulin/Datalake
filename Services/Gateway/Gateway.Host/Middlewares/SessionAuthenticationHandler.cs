using Datalake.Domain.Exceptions;
using Datalake.Gateway.Application.Interfaces;
using Datalake.Gateway.Application.Models;
using Datalake.Gateway.Host.Interfaces;
using Datalake.Shared.Hosting.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Datalake.Gateway.Host.Middlewares;

public class SessionAuthenticationHandler(
	ISessionTokenExtractor sessionTokenExtractor,
	ISessionsService service,
	IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		// Извлекаем токен сессии
		var sessionToken = sessionTokenExtractor.ExtractToken(Request);

		// Проверяем сессию
		SessionInfo sessionInfo;
		try
		{
			sessionInfo = await service.GetAsync(sessionToken);

			// Создаем principal с claims
			var claims = new[]
			{
				new Claim(ClaimTypes.Sid, sessionInfo.UserGuid.ToString()),
				new Claim("SessionToken", sessionToken),
			};

			var identity = new ClaimsIdentity(claims, Scheme.Name);
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);

			// Добавляем UserGuid в заголовки для downstream сервисов
			Request.Headers[Headers.UserGuidHeader] = sessionInfo.UserGuid.ToString();

			return AuthenticateResult.Success(ticket);
		}
		catch (AppException ex)
		{
			return AuthenticateResult.Fail(ex.Message);
		}
		catch (Exception ex) // Общие исключения
		{
			Logger.LogError(ex, "Непредвиденная ошибка при проверке сессии");
			return AuthenticateResult.Fail("Непредвиденная ошибка при проверке сессии");
		}
	}
}
