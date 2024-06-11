using System.Net.Http.Json;

namespace DatalakeServer.TestRunner.Extensions
{
	public static class HttpClientExtension
	{
		static async Task HandleUnsuccessCodeAsync(HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
				throw new Exception(await response.Content.ReadAsStringAsync());
		}

		public static async Task PostAsync(this HttpClient httpClient, string url, object data)
		{
			var response = await httpClient.PostAsJsonAsync(url, data);
			await HandleUnsuccessCodeAsync(response);
		}

		public static async Task<T> PostAsync<T>(this HttpClient httpClient, string url, object data)
		{
			var response = await httpClient.PostAsJsonAsync(url, data);
			await HandleUnsuccessCodeAsync(response);

			T result = await response.Content.ReadFromJsonAsync<T>()
				?? throw new Exception("В ответе ничего не пришло");

			return result;
		}


		public static async Task GetAsync(this HttpClient httpClient, string url)
		{
			var response = await httpClient.GetAsync(url);
			await HandleUnsuccessCodeAsync(response);
		}

		public static async Task<T> GetAsync<T>(this HttpClient httpClient, string url)
		{
			var response = await httpClient.GetAsync(url);
			await HandleUnsuccessCodeAsync(response);

			T result = await response.Content.ReadFromJsonAsync<T>()
				?? throw new Exception("В ответе ничего не пришло");

			return result;
		}


		public static async Task PutAsync(this HttpClient httpClient, string url, object data)
		{
			var response = await httpClient.PutAsJsonAsync(url, data);
			await HandleUnsuccessCodeAsync(response);
		}

		public static async Task<T> PutAsync<T>(this HttpClient httpClient, string url, object data)
		{
			var response = await httpClient.PutAsJsonAsync(url, data);
			await HandleUnsuccessCodeAsync(response);

			T result = await response.Content.ReadFromJsonAsync<T>()
				?? throw new Exception("В ответе ничего не пришло");

			return result;
		}


		public static async Task DeleteAsync(this HttpClient httpClient, string url)
		{
			var response = await httpClient.DeleteAsync(url);
			await HandleUnsuccessCodeAsync(response);
		}

		public static async Task<T> DeleteAsync<T>(this HttpClient httpClient, string url)
		{
			var response = await httpClient.DeleteAsync(url);
			await HandleUnsuccessCodeAsync(response);

			T result = await response.Content.ReadFromJsonAsync<T>()
				?? throw new Exception("В ответе ничего не пришло");

			return result;
		}
	}
}
