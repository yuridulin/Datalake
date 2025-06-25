using Datalake.Database.InMemory;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Server.Controllers.Base
{
	/// <summary>
	/// Базовый контроллер приложения
	/// </summary>
	public class ApiControllerBase(
		DatalakeDerivedDataStore _derivedDataStore) : ControllerBase
	{
		/// <summary>
		/// Ссылка на хранилище зависимых данных
		/// </summary>
		protected DatalakeDerivedDataStore DerivedDataStore => _derivedDataStore;

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
					if (Guid.TryParse(raw, out var guid))
					{
						user.UnderlyingUser = _derivedDataStore.Access.Get(guid);
					}
				}

				return user;
			}

			throw new NotFoundException(message: "данные о сессии");
		}
	}
}
