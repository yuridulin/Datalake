using Datalake.Database;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.Services.Auth;
using Datalake.Server.Services.Maintenance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;

namespace Datalake.Server.Controllers;

/// <inheritdoc />
public class ValuesController(
	DatalakeContext db,
	AuthenticationService authenticator,
	ValuesRepository valuesRepository,
	TagsStateService tagsStateService/*,
	RequestsStateService requestsStateService*/) : ValuesControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<List<ValuesResponse>>> GetAsync(
		[BindRequired, FromBody] ValuesRequest[] requests)
	{
		var user = authenticator.Authenticate(HttpContext);

		var sw = Stopwatch.StartNew();
		var responses = await valuesRepository.GetValuesAsync(db, user, requests);
		sw.Stop();

		tagsStateService.UpdateTagState(requests);
		//requestsStateService.RecordBatch(requests, sw.Elapsed, responses);

		return responses;
	}

	/// <inheritdoc />
	public override async Task<ActionResult<List<ValuesTagResponse>>> WriteAsync(
		[BindRequired, FromBody] ValueWriteRequest[] requests)
	{
		var user = authenticator.Authenticate(HttpContext);

		var responses = await valuesRepository.WriteManualValuesAsync(db, user, requests);

		return responses;
	}
}