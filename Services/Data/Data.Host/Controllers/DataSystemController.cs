using Datalake.Data.Application.Features.DataCollection.Commands.ManualRestartCollection;
using Datalake.Shared.Hosting.AbstractControllers.Data;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Data.Host.Controllers;

public class DataSystemController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : DataSystemControllerBase
{
	public override async Task<ActionResult<bool>> RestartCollectionAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IManualRestartCollectionHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
		}, ct);

		return data;
	}
}
