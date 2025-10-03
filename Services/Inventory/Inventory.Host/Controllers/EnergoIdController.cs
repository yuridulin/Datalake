using Datalake.Inventory.Api.Models.Users;
using Datalake.Inventory.Application.Features.EnergoId.Commands.ReloadEnergoId;
using Datalake.Inventory.Application.Features.EnergoId.Queries.GetEnergoId;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Inventory.Host.Controllers;

/// <summary>
/// Учетные записи
/// </summary>
[ApiController]
[Route("api/v1/energo-id")]
public class EnergoIdController(IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка пользователей, определенных на сервере EnergoId
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список учетных записей EnergoId с отметкой на каждой, за какой учетной записью приложения закреплена</returns>
	[HttpGet]
	public async Task<ActionResult<UserEnergoIdInfo[]>> GetEnergoIdAsync(
		[FromServices] IGetEnergoIdHandler handler,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new() { User = user }, ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Обновление данных из EnergoId
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut]
	public async Task<ActionResult> UpdateEnergoIdAsync(
		[FromServices] IReloadEnergoIdHandler handler,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new() { User = user }, ct);

		return NoContent();
	}
}