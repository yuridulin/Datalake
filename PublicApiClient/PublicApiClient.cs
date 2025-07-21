using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Blocks;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.Users;
using Datalake.PublicApi.Models.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Datalake.PublicApiClient;

/// <summary>
/// Клиент для работы с сервером Datalake
/// </summary>
public abstract class DatalakePublicApiClient : ControllerBase
{
	/// <summary>
	/// Создание нового клиента публичного API
	/// </summary>
	/// <param name="baseUri">Путь к серверу Datalake</param>
	/// <param name="token">API ключ для доступа</param>
	public DatalakePublicApiClient(ILogger logger, string baseUri, string token)
	{
		_baseUri = baseUri;
		_token = token;
		_logger = logger;

		_client = new HttpClient
		{
			BaseAddress = new Uri(_baseUri)
		};
		_client.DefaultRequestHeaders.Add(AuthConstants.TokenHeader, _token);

		_logger.LogDebug("Datalake:\n\t{base}\n\t[{token}]", _client.BaseAddress, token);
	}

	private string _baseUri;
	private string _token;
	private HttpClient _client;
	private ILogger _logger;

	const string Tags = "api/tags";
	const string Values = "api/tags/values";
	const string Blocks = "api/blocks";
	const string BlocksTree = "api/blocks/tree";
	const string Users = "api/users";

	/// <summary>
	/// Получение списка тегов, включая информацию о источниках и настройках получения данных
	/// </summary>
	/// <param name="sourceId">Идентификатор источника. Если указан, будут выбраны теги только этого источника</param>
	/// <param name="id">Список локальных идентификаторов тегов</param>
	/// <param name="names">Список текущих наименований тегов</param>
	/// <param name="guids">Список глобальных идентификаторов тегов</param>
	/// <returns>Плоский список объектов информации о тегах</returns>
	[HttpGet(Tags)]
	public virtual async Task<ActionResult<TagInfo[]>> GetTagsAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? id,
		[FromQuery] string[]? names,
		[FromQuery] Guid[]? guids)
	{
		var queryParams = new Dictionary<string, string>();

		if (sourceId.HasValue)
			queryParams["sourceId"] = sourceId.Value.ToString();

		if (id?.Length > 0)
			queryParams["id"] = string.Join(",", id);

		if (names?.Length > 0)
			queryParams["names"] = string.Join(",", names);

		if (guids?.Length > 0)
			queryParams["guids"] = string.Join(",", guids);

		var queryString = QueryHelpers.AddQueryString(Tags, queryParams);

		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Get,
			RequestUri = new Uri(queryString, UriKind.Relative),
		};

		_logger.LogDebug("Datalake > {name}: {method} {uri}", nameof(GetTagsAsync), request.Method, request.RequestUri);

		ProcessRequest(request);

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	/// <summary>
	/// Получение значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов с настройками</param>
	/// <returns>Список ответов на запросы</returns>
	[HttpPost(Values)]
	public virtual async Task<ActionResult<ValuesResponse[]>> GetValuesAsync(
		[BindRequired, FromBody] ValuesRequest[] requests)
	{
		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Post,
			RequestUri = new Uri(Values, UriKind.Relative),
			Content = new StringContent(
				JsonSerializer.Serialize(requests),
				encoding: Encoding.UTF8,
				mediaType: MediaTypeNames.Application.Json),
		};

		_logger.LogDebug("Datalake > {name}: {method} {uri}", nameof(GetValuesAsync), request.Method, request.RequestUri);

		ProcessRequest(request);

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	/// <summary>
	/// Запись значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов на изменение</param>
	/// <returns>Список измененных начений</returns>
	[HttpPut(Values)]
	public virtual async Task<ActionResult<List<ValuesTagResponse>>> WriteValuesAsync(
		[BindRequired, FromBody] ValueWriteRequest[] requests)
	{
		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Put,
			RequestUri = new Uri(Values, UriKind.Relative),
			Content = new StringContent(
				JsonSerializer.Serialize(requests),
				encoding: Encoding.UTF8,
				mediaType: MediaTypeNames.Application.Json),
		};

		_logger.LogDebug("Datalake > {name}: {method} {uri}", nameof(WriteValuesAsync), request.Method, request.RequestUri);

		ProcessRequest(request);

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	/// <summary>
	/// Получение списка блоков с базовой информацией о них
	/// </summary>
	/// <returns>Список блоков</returns>
	[HttpGet(Blocks)]
	public virtual async Task<ActionResult<BlockWithTagsInfo[]>> GetBlocksAsync()
	{
		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Get,
			RequestUri = new Uri(Blocks, UriKind.Relative),
		};

		_logger.LogWarning("Datalake > {name}: {method} {uri}", nameof(GetBlocksAsync), request.Method, new Uri(_client.BaseAddress!, request.RequestUri));

		ProcessRequest(request);

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	/// <summary>
	/// Получение иерархической структуры всех блоков
	/// </summary>
	/// <returns>Список обособленных блоков с вложенными блоками</returns>
	[HttpGet(BlocksTree)]
	public virtual async Task<ActionResult<BlockTreeInfo[]>> GetBlocksTreeAsync()
	{
		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Get,
			RequestUri = new Uri(BlocksTree, UriKind.Relative),
		};

		_logger.LogDebug("Datalake > {name}: {method} {uri}", nameof(GetBlocksTreeAsync), request.Method, request.RequestUri);

		ProcessRequest(request);

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	/// <summary>
	/// Получение списка пользователей
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet(Users)]
	public virtual async Task<ActionResult<UserInfo[]>> GetUsersAsync()
	{
		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Get,
			RequestUri = new Uri(Users, UriKind.Relative),
		};

		_logger.LogDebug("Datalake > {name}: {method} {uri}", nameof(GetUsersAsync), request.Method, request.RequestUri);

		ProcessRequest(request);

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	/// <summary>
	/// Создание пользователя на основании переданных данных
	/// </summary>
	/// <param name="userAuthRequest">Данные нового пользователя</param>
	/// <returns>Идентификатор пользователя</returns>
	[HttpPost(Users)]
	public virtual async Task<ActionResult<Guid>> CreateUserAsync(
		[BindRequired, FromBody] UserCreateRequest userAuthRequest)
	{
		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Post,
			RequestUri = new Uri(Users, UriKind.Relative),
			Content = new StringContent(
				JsonSerializer.Serialize(userAuthRequest),
				encoding: Encoding.UTF8,
				mediaType: MediaTypeNames.Application.Json),
		};

		_logger.LogDebug("Datalake > {name}: {method} {uri}", nameof(CreateUserAsync), request.Method, request.RequestUri);

		ProcessRequest(request);

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	protected abstract void ProcessRequest(HttpRequestMessage request);

	protected virtual void SetUnderlyingUser(HttpRequestMessage request, Guid? userGuid = null)
	{
		if (userGuid == null)
			return;

		request.Headers.Add(AuthConstants.UnderlyingUserGuidHeader, userGuid.ToString());
		_logger.LogDebug("Datalake > as user [{user}]", userGuid);
	}

	private async Task<FileStreamResult> ReturnStreamResponse(
		HttpResponseMessage response)
	{
		var stream = await response.Content.ReadAsStreamAsync();
		HttpContext.Response.StatusCode = (int)response.StatusCode;

		return new FileStreamResult(stream, "application/json")
		{
		};
	}
}