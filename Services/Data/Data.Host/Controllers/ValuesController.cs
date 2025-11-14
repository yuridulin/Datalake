using Datalake.Contracts.Models.Data.Values;
using Datalake.Data.Application.Features.Values.Commands.ManualWriteValues;
using Datalake.Data.Application.Features.Values.Queries.GetValues;
using Datalake.Shared.Hosting.Controllers.Data;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Data.Host.Controllers;

public class ValuesController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : DataValuesControllerBase
{
	public override async Task<ActionResult<IEnumerable<ValuesResponse>>> GetAsync(
		[BindRequired, FromBody] IEnumerable<ValuesRequest> requests,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetValuesHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			Requests = requests,
		}, ct);

		return Ok(data);
	}

	public override async Task<ActionResult<IEnumerable<ValuesTagResponse>>> WriteAsync(
		[BindRequired, FromBody] IEnumerable<ValueWriteRequest> requests,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IManualWriteValuesHandler>();
		var result = await handler.HandleAsync(new()
		{
			User = user,
			Requests = requests,
		}, ct);

		return Ok(result);
	}
}