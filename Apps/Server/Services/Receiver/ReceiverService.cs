using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.Services.Receiver.Models;
using Datalake.Server.Services.Receiver.Models.Inopc;
using Datalake.Server.Services.Receiver.Models.Inopc.Enums;
using Datalake.Server.Services.Receiver.Models.OldDatalake;
using Newtonsoft.Json;
using System.Text.Json;
using OldDatalakeTagQuality = Datalake.Server.Services.Receiver.Models.OldDatalake.TagQuality;
using OldDatalakeTagType = Datalake.Server.Services.Receiver.Models.OldDatalake.TagType;
using TagQuality = Datalake.PublicApi.Enums.TagQuality;
using TagType = Datalake.PublicApi.Enums.TagType;

namespace Datalake.Server.Services.Receiver;

/// <summary>
/// Служба получения данных из источников по сети
/// </summary>
/// <param name="logger">Служба сообщений</param>
public class ReceiverService(ILogger<ReceiverService> logger)
{
	private CancellationTokenSource cancellationTokenSource = new();

	private static JsonSerializerOptions JsonOptions = new()
	{
		Converters = { new JsonObjectConverter(), }
	};

	private static HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(1), };

	/// <summary>
	/// Универсальное получение данных из удаленного источника
	/// </summary>
	/// <param name="type">Тип источника данных</param>
	/// <param name="address">Адрес</param>
	/// <returns>Ответ с данными</returns>
	public async Task<ReceiveResponse> GetItemsFromSourceAsync(SourceType type, string? address)
	{
		if (string.IsNullOrEmpty(address))
		{
			return new ReceiveResponse
			{
				IsConnected = false,
				Tags = [],
				Timestamp = DateFormats.GetCurrentDateTime(),
			};
		}

		return type switch
		{
			SourceType.Inopc => await AskInopc([], address),
			SourceType.Datalake => await AskOldDatalake([], address),
			_ => await AskDatalake([], address),
		};
	}

	/// <summary>
	/// Запрос данных из сервера INOPC
	/// </summary>
	/// <param name="tags">Список имен запрашиваемых тегов</param>
	/// <param name="address">Адрес сервера</param>
	/// <returns>Ответ с данными</returns>
	public async Task<ReceiveResponse> AskInopc(string[] tags, string address)
	{
		ReceiveResponse response = new()
		{
			IsConnected = false,
			Tags = [],
		};

		InopcResponse? inopcResponse = null;

		try
		{
			var request = new InopcRequest
			{
				Tags = tags
			};

			var answer = await HttpClient.PostAsJsonAsync("http://" + address + ":81/api/storage/read", request, cancellationTokenSource.Token);
			if (answer.IsSuccessStatusCode)
			{
				inopcResponse = await answer.Content.ReadFromJsonAsync<InopcResponse>(JsonOptions);
			}
		}
		catch { }

		response.Timestamp = inopcResponse?.Timestamp ?? DateFormats.GetCurrentDateTime();
		if (inopcResponse != null)
		{
			response.IsConnected = true;
			response.Tags = inopcResponse.Tags
				.Select(x => new ReceiveRecord
				{
					Name = x.Name,
					Quality = x.Quality switch
					{
						InopcTagQuality.Good => TagQuality.Good,
						InopcTagQuality.Good_ManualWrite => TagQuality.Good_ManualWrite,
						InopcTagQuality.Bad => TagQuality.Bad,
						InopcTagQuality.Bad_NoConnect => TagQuality.Bad_NoConnect,
						InopcTagQuality.Bad_NoValues => TagQuality.Bad_NoValues,
						InopcTagQuality.Bad_ManualWrite => TagQuality.Bad_ManualWrite,
						_ => TagQuality.Unknown,
					},
					Type = x.Type switch
					{
						InopcTagType.Boolean => TagType.Boolean,
						InopcTagType.String => TagType.String,
						_ => TagType.Number,
					},
					Value = x.Value,
				})
				.ToArray();
		}

		return response;
	}

	/// <summary>
	/// Запрос данных из ноды Datalake, версия .NET Framework
	/// </summary>
	/// <param name="tags">Список названий запрашиваемых тегов</param>
	/// <param name="address">Адрес ноды</param>
	/// <returns>Ответ с данными</returns>
	public async Task<ReceiveResponse> AskOldDatalake(string[] tags, string address)
	{
		ReceiveResponse response = new()
		{
			IsConnected = false,
			Tags = [],
		};

		HistoryResponse[]? historyResponses = null;

		try
		{
			var request = new
			{
				Request = new LiveRequest
				{
					TagNames = [.. tags],
				}
			};

			var answer = await HttpClient.PostAsJsonAsync("http://" + address + ":83/api/tags/live", request);
			var content = await answer.Content.ReadAsStringAsync();
			historyResponses = JsonConvert.DeserializeObject<HistoryResponse[]>(content);
		}
		catch { }

		response.Timestamp = DateFormats.GetCurrentDateTime();
		if (historyResponses != null)
		{
			response.IsConnected = true;
			response.Tags = historyResponses
				.SelectMany(t => t.Values.Select(v => new ReceiveRecord
				{
					Name = t.TagName,
					Type = t.Type switch
					{
						OldDatalakeTagType.Number => TagType.Number,
						OldDatalakeTagType.Boolean => TagType.Boolean,
						_ => TagType.String,
					},
					Quality = v.Quality switch
					{
						OldDatalakeTagQuality.Bad => TagQuality.Bad,
						OldDatalakeTagQuality.Bad_NoConnect => TagQuality.Bad_NoConnect,
						OldDatalakeTagQuality.Bad_NoValues => TagQuality.Bad_NoValues,
						OldDatalakeTagQuality.Bad_ManualWrite => TagQuality.Bad_ManualWrite,
						OldDatalakeTagQuality.Good => TagQuality.Good,
						OldDatalakeTagQuality.Good_ManualWrite => TagQuality.Good_ManualWrite,
						_ => TagQuality.Unknown,
					},
					Value = v.Value,
				}))
				.ToArray();
		}

		return response;
	}

	/// <summary>
	/// Запрос данных из ноды Datalake
	/// </summary>
	/// <param name="tags">Список названий запрашиваемых тегов</param>
	/// <param name="address">Адрес ноды</param>
	/// <returns>Ответ с данными</returns>
	public async Task<ReceiveResponse> AskDatalake(Guid[] tags, string address)
	{
		ReceiveResponse response = new()
		{
			IsConnected = false,
			Tags = [],
		};

		ValuesResponse? historyResponse = null;

		try
		{
			var request = new
			{
				Request = new ValuesRequest
				{
					RequestKey = "1",
					Tags = tags,
				}
			};

			var answer = await HttpClient.PostAsJsonAsync($"http://{address}:81/{ValuesControllerBase.LiveUrl}", request);
			var historyResponses = await answer.Content.ReadFromJsonAsync<ValuesResponse[]>(JsonOptions);
			historyResponse = historyResponses?.FirstOrDefault();
		}
		catch (Exception ex)
		{
			logger.LogWarning("Ask Old Datalake {address}: {err}", address, ex.Message);
		}

		response.Timestamp = DateFormats.GetCurrentDateTime();
		if (historyResponse != null)
		{
			response.IsConnected = true;
			response.Tags = historyResponse.Tags
				.SelectMany(t => t.Values.Select(v => new ReceiveRecord
				{
					Name = t.Guid.ToString(),
					Type = t.Type,
					Quality = v.Quality,
					Value = v.Value,
				}))
				.ToArray();
		}

		return response;
	}
}