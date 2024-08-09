using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.Values;
using Datalake.Server.Controllers;
using Datalake.Server.Services.Receiver.Models;
using Datalake.Server.Services.Receiver.Models.Inopc;
using Datalake.Server.Services.Receiver.Models.Inopc.Enums;

namespace Datalake.Server.Services.Receiver;

/// <summary>
/// Служба получения данных из источников по сети
/// </summary>
/// <param name="logger">Служба сообщений</param>
public class ReceiverService(ILogger<ReceiverService> logger)
{
	CancellationTokenSource cancellationTokenSource = new();

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
				Tags = [],
				Timestamp = DateTime.Now,
			};
		}

		return type switch
		{
			SourceType.Inopc => await AskInopc([], address),
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
		logger.LogDebug("Ask iNOPC with address: {address}", address);

		var request = new InopcRequest
		{
			Tags = tags
		};

		ReceiveResponse response = new()
		{
			Timestamp = DateTime.Now,
			Tags = [],
		};

		using var client = new HttpClient();
		client.Timeout = TimeSpan.FromSeconds(1);

		InopcResponse? inopcResponse = null;

		try
		{
			var answer = await client.PostAsJsonAsync("http://" + address + ":81/api/storage/read", request, cancellationTokenSource.Token);
			if (answer.IsSuccessStatusCode)
			{
				inopcResponse = await answer.Content.ReadFromJsonAsync<InopcResponse>();
			}
		}
		catch
		{
			logger.LogDebug("Ask iNOPC with address: {address} fail", address);
		}

		if (inopcResponse != null)
		{
			response = new ReceiveResponse
			{
				Timestamp = inopcResponse.Timestamp,
				Tags = inopcResponse.Tags
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
					.ToArray(),
			};
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
		logger.LogDebug("Ask old datalake with address: {address}", address);

		var request = new
		{
			Request = new Models.OldDatalake.LiveRequest
			{
				TagNames = [.. tags],
			}
		};

		using var client = new HttpClient();
		client.Timeout = TimeSpan.FromSeconds(1);

		List<Models.OldDatalake.HistoryResponse>? historyResponses = null;

		try
		{
			var answer = await client.PostAsJsonAsync("http://" + address + ":83/api/tags/live", request);
			historyResponses = await answer.Content.ReadFromJsonAsync<List<Models.OldDatalake.HistoryResponse>>();
		}
		catch
		{
			logger.LogDebug("Ask old datalake with address: {address} fail", address);
		}

		historyResponses ??= [];

		var response = new ReceiveResponse
		{
			Timestamp = DateTime.Now,
			Tags = historyResponses
					.SelectMany(t => t.Values.Select(v => new ReceiveRecord
					{
						Name = t.TagName,
						Type = t.Type switch
						{
							Models.OldDatalake.TagType.Number => TagType.Number,
							Models.OldDatalake.TagType.Boolean => TagType.Boolean,
							_ => TagType.String,
						},
						Quality = v.Quality switch
						{
							Models.OldDatalake.TagQuality.Bad => TagQuality.Bad,
							Models.OldDatalake.TagQuality.Bad_NoConnect => TagQuality.Bad_NoConnect,
							Models.OldDatalake.TagQuality.Bad_NoValues => TagQuality.Bad_NoValues,
							Models.OldDatalake.TagQuality.Bad_ManualWrite => TagQuality.Bad_ManualWrite,
							Models.OldDatalake.TagQuality.Good => TagQuality.Good,
							Models.OldDatalake.TagQuality.Good_ManualWrite => TagQuality.Good_ManualWrite,
							_ => TagQuality.Unknown,
						},
						Value = v.Value,
					}))
					.ToArray(),
		};

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
		logger.LogDebug("Ask datalake with address: {address}", address);

		var request = new
		{
			Request = new ValuesRequest
			{
				RequestKey = "1",
				Tags = tags,
			}
		};

		using var client = new HttpClient();
		client.Timeout = TimeSpan.FromSeconds(1);

		List<ValuesResponse>? historyResponses = null;

		try
		{
			var answer = await client.PostAsJsonAsync("http://" + address + ":81/" + ValuesController.LiveUrl, request);
			historyResponses = await answer.Content.ReadFromJsonAsync<List<ValuesResponse>>();
		}
		catch
		{
			logger.LogDebug("Ask datalake with address: {address} fail", address);
		}

		historyResponses ??= [];

		var historyResponse = historyResponses.FirstOrDefault();

		var response = new ReceiveResponse
		{
			Timestamp = DateTime.Now,
			Tags = historyResponse != null
				? historyResponse.Tags
					.SelectMany(t => t.Values.Select(v => new ReceiveRecord
					{
						Name = t.Guid.ToString(),
						Type = t.Type,
						Quality = v.Quality,
						Value = v.Value,
					}))
					.ToArray()
				: [],
		};

		return response;
	}
}
