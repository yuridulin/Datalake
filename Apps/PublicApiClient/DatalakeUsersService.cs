using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Users;
using Microsoft.Extensions.Logging;

namespace Datalake.PublicApiClient;

public class DatalakeApiUsersService
{
	public DatalakeApiUsersService(
		ILogger logger,
		IHttpClientFactory httpClientFactory,
		string baseUri,
		string accessToken)
	{
		_logger = logger;
		_httpClient = httpClientFactory.CreateClient("DatalakeUserService");

		_httpClient.BaseAddress = new Uri(baseUri);

		_httpClient.DefaultRequestHeaders.Remove(AuthConstants.TokenHeader);
		_httpClient.DefaultRequestHeaders.Add(AuthConstants.TokenHeader, accessToken);
	}

	private readonly HttpClient _httpClient;
	private readonly ILogger _logger;

	public Task<ApiUpstreamResponse<UserInfo>> CreateAsync(
		UserCreateRequest request,
		Guid? underlying = null)
	{
		_logger.LogDebug("Datalake API: create user");

		var extraHeaders = underlying.HasValue
			? new Dictionary<string, string>
			{
				[AuthConstants.UnderlyingUserGuidHeader] = underlying.Value.ToString()
			}
			: null;

		return ApiUpstreamService.PostAsync<UserCreateRequest, UserInfo>(_httpClient, "/api/users", request, extraHeaders);
	}
}
