using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Models.Values;
using DatalakeServer.ApiControllers;
using DatalakeServer.Services.Receiver.Models;
using DatalakeServer.Services.Receiver.Models.Inopc;
using DatalakeServer.Services.Receiver.Models.Inopc.Enums;

namespace DatalakeServer.Services.Receiver;

public class ReceiverService(ILogger<ReceiverService> logger)
{
	CancellationTokenSource cancellationTokenSource = new();

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

	public async Task<ReceiveResponse> AskDatalake(string[] tags, string address)
	{
		logger.LogDebug("Ask datalake with address: {address}", address);

		var request = new
		{
			Request = new ValuesRequest
			{
				TagNames = tags,
			}
		};

		using var client = new HttpClient();
		client.Timeout = TimeSpan.FromSeconds(1);

		List<ValuesResponse>? historyResponse = null;

		try
		{
			var answer = await client.PostAsJsonAsync("http://" + address + ":81/" + ValuesController.LiveUrl, request);
			historyResponse = await answer.Content.ReadFromJsonAsync<List<ValuesResponse>>();
		}
		catch
		{
			logger.LogDebug("Ask datalake with address: {address} fail", address);
		}

		historyResponse ??= [];

		var response = new ReceiveResponse
		{
			Timestamp = DateTime.Now,
			Tags = historyResponse
				.SelectMany(x => x.Values, (tag, value) => new ReceiveRecord
				{
					Name = tag.TagName,
					Quality = value.Quality,
					Value = value.Value,
					Type = tag.Type,
				})
				.ToArray()
		};

		return response;
	}
}
