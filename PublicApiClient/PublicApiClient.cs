using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Blocks;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.Users;
using Datalake.PublicApi.Models.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Datalake.PublicApi;

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
	public DatalakePublicApiClient(string baseUri, string token)
	{
		_baseUri = baseUri;
		_token = token;

		_client = new HttpClient
		{
			BaseAddress = new Uri(_baseUri)
		};
		_client.DefaultRequestHeaders.Add(AuthConstants.TokenHeader, _token);
	}

	private string _baseUri;
	private string _token;
	private HttpClient _client;

	const string Tags = "tags";
	const string Values = "tags/values";
	const string Blocks = "blocks";
	const string BlocksTree = "blocks/tree";
	const string Users = "users";

	/// <summary>
	/// Получение списка тегов, включая информацию о источниках и настройках получения данных
	/// </summary>
	/// <param name="sourceId">Идентификатор источника. Если указан, будут выбраны теги только этого источника</param>
	/// <param name="id">Список локальных идентификаторов тегов</param>
	/// <param name="names">Список текущих наименований тегов</param>
	/// <param name="guids">Список глобальных идентификаторов тегов</param>
	/// <returns>Плоский список объектов информации о тегах</returns>
	[HttpGet(Tags)]
	public async Task<ActionResult<TagInfo[]>> GetTagsAsync(
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

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	/// <summary>
	/// Получение значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов с настройками</param>
	/// <returns>Список ответов на запросы</returns>
	[HttpPost(Values)]
	public async Task<ActionResult<ValuesResponse[]>> GetValuesAsync(
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

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	/// <summary>
	/// Запись значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов на изменение</param>
	/// <returns>Список измененных начений</returns>
	[HttpPut(Values)]
	public async Task<ActionResult<ValuesTagResponse>> WriteValuesAsync(
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

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	/// <summary>
	/// Получение списка блоков с базовой информацией о них
	/// </summary>
	/// <returns>Список блоков</returns>
	[HttpGet(Blocks)]
	public async Task<ActionResult<BlockWithTagsInfo[]>> GetBlocksAsync()
	{
		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Get,
			RequestUri = new Uri(Blocks, UriKind.Relative),
		};

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	/// <summary>
	/// Получение иерархической структуры всех блоков
	/// </summary>
	/// <returns>Список обособленных блоков с вложенными блоками</returns>
	[HttpGet(BlocksTree)]
	public async Task<ActionResult<BlockTreeInfo[]>> GetBlocksTreeAsync()
	{
		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Get,
			RequestUri = new Uri(BlocksTree, UriKind.Relative),
		};

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	/// <summary>
	/// Получение списка пользователей
	/// </summary>
	/// <returns>Список пользователей</returns>
	[HttpGet(Users)]
	public async Task<ActionResult<UserInfo[]>> GetUsersAsync()
	{
		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Get,
			RequestUri = new Uri(Users, UriKind.Relative),
		};

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
	}

	/// <summary>
	/// Создание пользователя на основании переданных данных
	/// </summary>
	/// <param name="userAuthRequest">Данные нового пользователя</param>
	/// <returns>Идентификатор пользователя</returns>
	[HttpPost(Users)]
	public async Task<ActionResult<Guid>> CreateUserAsync(
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

		var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		return await ReturnStreamResponse(response);
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