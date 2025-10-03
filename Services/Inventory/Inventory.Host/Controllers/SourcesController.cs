using Datalake.Inventory.Api.Models.Sources;
using Datalake.Inventory.Application.Features.Sources.Commands.CreateSource;
using Datalake.Inventory.Application.Features.Sources.Commands.DeleteSource;
using Datalake.Inventory.Application.Features.Sources.Commands.UpdateSource;
using Datalake.Inventory.Application.Features.Sources.Queries.GetSource;
using Datalake.Inventory.Application.Features.Sources.Queries.GetSources;
using Datalake.Shared.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.App.Controllers;

/// <summary>
/// Источники данных
/// </summary>
[ApiController]
[Route("api/v1/sources")]
public class SourcesController(IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание источника на основе переданных данных
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="request">Данные нового источника</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор источника</returns>
	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[FromServices] ICreateSourceHandler handler,
		[FromBody] SourceInfo? request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var result = await handler.HandleAsync(new()
		{
			User = user,
			Address = request?.Address,
			Description = request?.Description,
			Name = request?.Name,
			Type = request?.Type,
		}, ct);

		return Ok(result);
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение данных о источнике
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Данные о источнике</returns>
	public async Task<ActionResult<SourceInfo>> GetAsync(
		[FromServices] IGetSourceHandler handler,
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new() { User = user, SourceId = sourceId }, ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка источников
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="withCustom">Включить ли в список системные источники</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список источников</returns>
	public async Task<ActionResult<IEnumerable<SourceInfo>>> GetAllAsync(
		[FromServices] IGetSourcesHandler handler,
		[FromQuery] bool withCustom = false,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new() { User = user, WithCustom = withCustom }, ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение источника
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="request">Новые данные источника</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{sourceId}")]
	public async Task<ActionResult> UpdateAsync(
		[FromServices] IUpdateSourceHandler handler,
		[BindRequired, FromRoute] int sourceId,
		[BindRequired, FromBody] SourceUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new()
		{
			User = user,
			SourceId = sourceId,
			Name = request.Name,
			Description = request.Description,
			Address = request.Address,
			Type = request.Type,
			IsDisabled = request.IsDisabled,
		}, ct);

		return Ok();
	}

	/// <summary>
	/// <see cref="HttpMethod.Delete" />: Удаление источника
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{sourceId}")]
	public async Task<ActionResult> DeleteAsync(
		[FromServices] IDeleteSourceHandler handler,
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new()
		{
			User = user,
			SourceId = sourceId
		}, ct);

		return Ok();
	}
}