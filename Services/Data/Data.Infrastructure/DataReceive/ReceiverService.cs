using Datalake.Data.Application.Interfaces;
using Datalake.Data.Application.Models;
using Datalake.Data.Infrastructure.DataReceive.Converters;
using Datalake.Data.Infrastructure.DataReceive.Inopc;
using Datalake.Data.Infrastructure.DataReceive.Inopc.Enums;
using Datalake.Domain.Enums;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Datalake.Data.Infrastructure.DataReceive;

[Singleton]
public class ReceiverService(ILogger<ReceiverService> logger) : IReceiverService
{
	private static JsonSerializerOptions JsonOptions { get; } = new()
	{
		Converters = { new JsonObjectConverter(), }
	};

	private static HttpClient HttpClient { get; } = new() { Timeout = TimeSpan.FromSeconds(1), };

	public async Task<RemoteResponseDto> AskInopcAsync(string[]? tags, string? address, int? port)
	{
		if (string.IsNullOrEmpty(address))
			throw new InfrastructureException("Адрес источника данных не указан");
		if (port is null)
			throw new InfrastructureException("Порт источника данных не указан");

		RemoteResponseDto response = new()
		{
			IsConnected = false,
			Tags = [],
		};

		InopcResponse? inopcResponse = null;

		try
		{
			var request = new InopcRequest
			{
				Tags = tags ?? [],
			};

			var answer = await HttpClient.PostAsJsonAsync($"http://{address}:{port}/api/storage/read", request);
			if (answer.IsSuccessStatusCode)
			{
				inopcResponse = await answer.Content.ReadFromJsonAsync<InopcResponse>(JsonOptions);
			}
		}
		catch (OperationCanceledException) { }
		catch (Exception ex)
		{
			if (logger.IsEnabled(LogLevel.Warning))
				logger.LogWarning("Не удалось получить данные. Адрес: {url}, ошибка: {err}", address, ex.Message);
		}

		response.Timestamp = inopcResponse?.Timestamp ?? DateTime.UtcNow;
		if (inopcResponse != null)
		{
			response.IsConnected = true;
			response.Tags = inopcResponse.Tags
				.Select(x => new RemoteRequestItemDto
				{
					Name = x.Name,
					Quality = x.Quality switch
					{
						InopcTagQuality.Good => TagQuality.Good,
						InopcTagQuality.Good_ManualWrite => TagQuality.Good_ManualWrite,
						InopcTagQuality.Bad => TagQuality.Bad_NoValues,
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
}
