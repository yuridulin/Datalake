using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Auth;

namespace Datalake.InventoryService.Api;

/// <summary>
/// Расширение для работы с контекстом запроса
/// </summary>
public static class HttpContextExtension
{
	/// <summary>
	/// Добавление данных о сессии к запросу
	/// </summary>
	/// <param name="session">Сессия</param>
	/// <param name="response">Запрос</param>
	public static void AddSessionToResponse(this HttpResponse response, UserSessionInfo session)
	{
		response.Headers[AuthConstants.TokenHeader] = session.Token;
		response.Headers[AuthConstants.NameHeader] = Uri.EscapeDataString(session.AuthInfo.FullName);
		response.Headers[AuthConstants.GlobalAccessHeader] = session.AuthInfo.RootRule.Access.ToString();
	}
}
