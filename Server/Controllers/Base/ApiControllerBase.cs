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
			if (HttpContext.Items.TryGetValue(AuthConstants.ContextSessionKey, out var session))
			{
				if (session == null)
					throw new ForbiddenException(message: "требуется пройти аутентификацию");

				return (UserAuthInfo)session;
			}

			throw new NotFoundException(message: "данные о сессии");
		}
	}
}
