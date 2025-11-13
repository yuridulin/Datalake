using Datalake.Contracts.Public.Models.Sources;
using Datalake.Inventory.Application.Features.Sources.Commands.CreateSource;
using Datalake.Inventory.Application.Features.Sources.Commands.DeleteSource;
using Datalake.Inventory.Application.Features.Sources.Commands.UpdateSource;
using Datalake.Inventory.Application.Features.Sources.Queries.GetSource;
using Datalake.Inventory.Application.Features.Sources.Queries.GetSources;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

/// <summary>
/// Источники данных
/// </summary>
[ApiController]
[Route("api/sources")]
public class SourcesController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// Создание источника на основе переданных данных
	/// </summary>
	/// <param name="request">Данные нового источника</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор источника</returns>
	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[FromBody] SourceInfo? request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<ICreateSourceHandler>();
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
	/// Получение данных о источнике
	/// </summary>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Данные о источнике</returns>
	[HttpGet("{sourceId}")]
	public async Task<ActionResult<SourceInfo>> GetAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetSourceHandler>();
		var data = await handler.HandleAsync(new() { User = user, SourceId = sourceId }, ct);

		return Ok(data);
	}

	/// <summary>
	/// Получение списка источников
	/// </summary>
	/// <param name="withCustom">Включить ли в список системные источники</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список источников</returns>
	[HttpGet]
	public async Task<ActionResult<IEnumerable<SourceInfo>>> GetAllAsync(
		[FromQuery] bool withCustom = false,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetSourcesHandler>();
		var data = await handler.HandleAsync(new() { User = user, WithCustom = withCustom }, ct);

		return Ok(data);
	}

	/// <summary>
	/// Изменение источника
	/// </summary>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="request">Новые данные источника</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{sourceId}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int sourceId,
		[BindRequired, FromBody] SourceUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateSourceHandler>();
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
	/// Удаление источника
	/// </summary>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{sourceId}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IDeleteSourceHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			SourceId = sourceId
		}, ct);

		return Ok();
	}
}