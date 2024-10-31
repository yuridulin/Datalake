using Datalake.Database.Exceptions;
using Datalake.Database.Models.Auth;
using Datalake.Server.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Server.Controllers.Base
{
	/// <summary>
	/// Базовый контроллер приложения
	/// </summary>
	public class ApiControllerBase : ControllerBase
	{
		/// <summary>
		/// Аутентификация пользователя по сессионному токену из запроса
		/// </summary>
		/// <returns>Данные о пользователе</returns>
		/// <exception cref="ForbiddenException">Сессия не открыта</exception>
		/// <exception cref="NotFoundException">Сессия не найдена</exception>
		protected UserAuthInfo Authenticate()
		{
			if (HttpContext.Items.TryGetValue(AuthConstants.ContextSessionKey, out var sessionUserAuthInfo))
			{
				if (sessionUserAuthInfo == null)
					throw new ForbiddenException(message: "требуется пройти аутентификацию");

				var user = (UserAuthInfo)sessionUserAuthInfo;

				if (HttpContext.Request.Headers.TryGetValue(AuthConstants.UnderlyingUserGuidHeader, out var raw))
				{
					user.UnderlyingUserGuid = Guid.TryParse(raw, out var guid) ? guid : null;
				}

				return user;
			}

			throw new NotFoundException(message: "данные о сессии");
		}
	}
}
