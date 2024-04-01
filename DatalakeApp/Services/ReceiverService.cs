namespace DatalakeApp.Services
{
	public class ReceiverService
  {
		/*public async Task<DatalakeResponse> GetItemsFromSourceAsync(Source source)
		{
			if (source.Type == SourceType.Inopc)
			{
				if (!string.IsNullOrEmpty(source.Address))
					return await AskInopc([], source.Address);
			}

			if (source.Type == SourceType.Datalake)
			{
				if (!string.IsNullOrEmpty(source.Address))
					return await AskDatalake([], source.Address);
			}

			return new DatalakeResponse
			{
				Timestamp = DateTime.Now,
				Tags = [],
			};
		}

		static async Task<DatalakeResponse> AskInopc(string[] tags, string address)
		{
			var request = new DatalakeRequest
			{
				Tags = tags
			};

			var answer = await new HttpClient().PostAsJsonAsync("http://" + address + ":81/api/storage/read", request);
			var response = await answer.Content.ReadFromJsonAsync<DatalakeResponse>();

			response ??= new DatalakeResponse
			{
				Timestamp = DateTime.Now,
				Tags = [],
			};

			return response;
		}

		static async Task<DatalakeResponse> AskDatalake(string[] tags, string address)
		{
			var request = new
			{
				Request = new LiveRequest
				{
					TagNames = tags,
				}
			};

			var answer = await new HttpClient().PostAsJsonAsync("http://" + address + ":81/" + ValuesController.LiveUrl, request);
			var historyResponse = await answer.Content.ReadFromJsonAsync<List<HistoryResponse>>();

			historyResponse ??= [];

			var response = new DatalakeResponse
			{
				Timestamp = DateTime.Now,
				Tags = historyResponse
					.SelectMany(x => x.Values, (tag, value) => new DatalakeRecord
					{
						Name = tag.TagName,
						Quality = (ushort)value.Quality,
						Value = value.Value,
						Type = tag.Type,
					})
					.ToArray()
			};

			return response;
		}*/
	}
}
