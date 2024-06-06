using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Users;
using DatalakeServer.Constants;
using Microsoft.AspNetCore.Mvc;

namespace DatalakeServer.ApiControllers.Base
{
	public class ApiControllerBase : ControllerBase
	{
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
