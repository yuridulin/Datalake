using DatalakeApp.ApiControllers;
using DatalakeApp.Models.Receiver;
using DatalakeDatabase.ApiModels.Values;
using DatalakeDatabase.Enums;

namespace DatalakeApp.Services
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

		static async Task<ReceiveResponse> AskInopc(string[] tags, string address)
		{
			var request = new DatalakeRequest
			{
				Tags = tags
			};

			var answer = await new HttpClient().PostAsJsonAsync("http://" + address + ":81/api/storage/read", request);
			var response = await answer.Content.ReadFromJsonAsync<ReceiveResponse>();

			response ??= new ReceiveResponse
			{
				Timestamp = DateTime.Now,
				Tags = [],
			};

			return response;
		}

		static async Task<ReceiveResponse> AskDatalake(string[] tags, string address)
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
						Quality = (ushort)value.Quality,
						Value = value.Value,
						Type = tag.Type,
					})
					.ToArray()
			};

			return response;
		}
	}
}
