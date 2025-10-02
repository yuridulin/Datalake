using Datalake.DataService.Abstractions;
using Datalake.DataService.Services.Receiver.Models;
using Datalake.DataService.Services.Receiver.Models.Inopc;
using Datalake.DataService.Services.Receiver.Models.Inopc.Enums;
using Datalake.PrivateApi.Attributes;
using Datalake.PrivateApi.Converters;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.Values;
using System.Text.Json;
using TagQuality = Datalake.Contracts.Public.Enums.TagQuality;
using TagType = Datalake.Contracts.Public.Enums.TagType;

namespace Datalake.DataService.Services.Receiver;

[Singleton]
public class ReceiverService(ILogger<ReceiverService> logger) : IReceiverService
{
	private CancellationTokenSource cancellationTokenSource = new();

	private static JsonSerializerOptions JsonOptions = new()
	{
		Converters = { new JsonObjectConverter(), }
	};

	private static HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(1), };

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