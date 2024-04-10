using DatalakeApp.ApiControllers;
using DatalakeApp.Services.Receiver.Models;
using DatalakeApp.Services.Receiver.Models.Inopc;
using DatalakeApp.Services.Receiver.Models.Inopc.Enums;
using DatalakeDatabase.ApiModels.Values;
using DatalakeDatabase.Enums;

namespace DatalakeApp.Services.Receiver
{
	public class ReceiverService
	{
		public async Task<ReceiveResponse> GetItemsFromSourceAsync(SourceType type, string? address)
		{
			if (string.IsNullOrEmpty(address))
			{
				return new ReceiveResponse
				{
					Tags = [],
					Timestamp = DateTime.UtcNow,
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
			var request = new InopcRequest
			{
				Tags = tags
			};

			var answer = await new HttpClient().PostAsJsonAsync("http://" + address + ":81/api/storage/read", request);
			var inopcResponse = await answer.Content.ReadFromJsonAsync<InopcResponse>();
			if (inopcResponse == null)
			{
				return new ReceiveResponse
				{
					Timestamp = DateTime.UtcNow,
					Tags = [],
				};
			}

			var response = new ReceiveResponse
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

			return response;
		}

		public async Task<ReceiveResponse> AskDatalake(string[] tags, string address)
		{
			var request = new
			{
				Request = new ValuesRequest
				{
					TagNames = tags,
				}
			};

			var answer = await new HttpClient().PostAsJsonAsync("http://" + address + ":81/" + ValuesController.LiveUrl, request);
			var historyResponse = await answer.Content.ReadFromJsonAsync<List<ValuesResponse>>();

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
}
