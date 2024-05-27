using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace DatalakeServer.ApiControllers.Base
{
	public class ApiControllerBase : ControllerBase
	{
		protected UserAuthInfo Authenticate()
		{
			var user = (HttpContext.Items.TryGetValue("Session", out var session) ? session : null) ?? throw new ForbiddenException(message: "требуется пройти аутентификацию");

			return (UserAuthInfo)user;
		}
	}
}
