using Datalake.Gateway.Application.Features.Queries.GetUsersActivity;
using Datalake.Gateway.Host.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.Controllers;

/// <summary>
/// Метрики пользователей
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Gateway")]
[Route("api/v1/gateway/users")]
public class UsersController(
	IServiceProvider serviceProvider,
	ISessionTokenExtractor tokenExtractor) : ControllerBase
{
	/// <summary>
	/// Получение времени последней активности пользователей
	/// </summary>
	/// <param name="users">Идентификаторы запрошенных пользователей</param>
	[HttpPost("activity")]
	public async Task<ActionResult<IDictionary<Guid, DateTime?>>> GetActivityAsync(
		[FromBody, BindRequired] IEnumerable<Guid> users)
	{
		var token = tokenExtractor.ExtractToken(HttpContext);

		var handler = serviceProvider.GetRequiredService<IGetUsersActivityHandler>();
		var result = await handler.HandleAsync(new()
		{
			Token	= token,
			Users = users,
		});

		return Ok(result);
	}
}
