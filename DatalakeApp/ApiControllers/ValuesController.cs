using DatalakeApp.Models;
using DatalakeApp.Services;
using DatalakeDatabase.Enums;
using DatalakeDatabase.Extensions;
using DatalakeDatabase.Models;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace DatalakeApp.ApiControllers
{
	[ApiController]
	[Route("api/Tags/[controller]")]
	public class ValuesController(HistoryService historyService, CacheService cacheService) : ControllerBase
	{
		public const string LiveUrl = "api/Tags/values/live";

		[HttpPost("live")]
		public async Task<HistoryResponse[]> Live(
			[FromBody] LiveRequest request)
		{
			request.Tags ??= await historyService.Db.Tags
				.Where(x => request.TagNames.Length == 0 || request.TagNames.Contains(x.Name))
				.Select(x => x.Id)
				.ToArrayAsync();

			Tag[] tags = await historyService.Db.Tags
				.Where(x => request.Tags.Length == 0 || request.Tags.Contains(x.Id))
				.ToArrayAsync();

			Dictionary<int, TagHistory> live = cacheService
				.ReadValues(tags.Select(x => x.Id).ToArray())
				.ToDictionary(x => x.TagId, x => x);

			HistoryResponse[] values = tags
				.Select(x => new
				{
					Tag = x,
					Value = live[x.Id]
				})
				.Select(x => new HistoryResponse
				{
					Id = x.Tag.Id,
					TagName = x.Tag.Name,
					Type = x.Tag.Type,
					Func = AggregationFunc.List,
					Values =
					[
						new() {
							Date = x.Value.Date,
							Value = x.Value.GetTypedValue(x.Tag.Type),
							Quality = x.Value.Quality,
							Using = x.Value.Using,
						}
					]
				})
				.ToArray();

			return values;
		}

		[HttpPost("history")]
		public async Task<List<HistoryResponse>> History(
			[FromBody] HistoryRequest[] requests)
		{
			List<HistoryResponse> responses = [];

			foreach (var request in requests)
			{
				request.Tags ??= await historyService.Db.Tags
					.Where(x => request.TagNames.Length == 0 || request.TagNames.Contains(x.Name))
					.Select(x => x.Id)
					.ToArrayAsync();

				DateTime old, young;

				// Если не указывается ни одна дата, выполняется получение текущих значений. Не убирать!
				if (!request.Exact.HasValue && !request.Old.HasValue && !request.Young.HasValue)
				{
					responses.AddRange(await Live(request));
					continue;
				}
				else if (request.Exact.HasValue)
				{
					young = request.Exact.Value;
					old = request.Exact.Value;
				}
				else
				{
					young = request.Young ?? DateTime.Now;
					old = request.Old ?? young.Date;
				}

				var tags = await historyService.Db.Tags
					.Where(x => request.Tags.Length == 0 || request.Tags.Contains(x.Id))
					.ToArrayAsync();

				var data = await historyService.ReadHistoryValuesAsync(
					tags.Select(x => x.Id).ToArray(),
					old,
					young,
					Math.Max(0, request.Resolution));

				foreach (var item in data.GroupBy(x => x.TagId))
				{
					var tag = tags.FirstOrDefault(x => x.Id == item.Key);
					if (tag == null)
						continue;

					if (request.Func == AggregationFunc.List)
					{
						responses.Add(new HistoryResponse
						{
							Id = tag.Id,
							TagName = tag.Name,
							Type = tag.Type,
							Func = request.Func,
							Values = [.. item
								.Select(x => new HistoryRecord
								{
									Date = x.Date,
									Quality = x.Quality,
									Using = x.Using,
									Value = x.GetTypedValue(tag.Type),
								})
								.OrderBy(x => x.Date)],
						});
					}
					else if (tag.Type == TagType.Number)
					{
						var values = item
							.Where(x => x.Quality == TagQuality.Good || x.Quality == TagQuality.Good_ManualWrite)
							.Select(x => x.GetTypedValue(tag.Type) as float?)
							.ToList();

						if (values.Count > 0)
						{
							float? value = 0;
							try
							{
								switch (request.Func)
								{
									case AggregationFunc.Sum:
										value = values.Sum();
										break;
									case AggregationFunc.Avg:
										value = values.Average();
										break;
									case AggregationFunc.Min:
										value = values.Min();
										break;
									case AggregationFunc.Max:
										value = values.Max();
										break;
								}
							}
							catch
							{
							}

							responses.Add(new HistoryResponse
							{
								Id = tag.Id,
								TagName = tag.Name,
								Type = tag.Type,
								Func = request.Func,
								Values = [
										new HistoryRecord
										{
											Quality = TagQuality.Good,
											Using = TagUsing.Aggregated,
											Value = value,
										}
									]
							});
						}
						else
						{
							responses.Add(new HistoryResponse
							{
								Id = tag.Id,
								TagName = tag.Name,
								Type = tag.Type,
								Func = request.Func,
								Values = [
										new HistoryRecord
										{
											Quality = TagQuality.Bad_NoValues,
											Using = TagUsing.Aggregated,
											Value = 0,
										}
									]
							});
						}
					}
				}
			}

			return responses;
		}

		[HttpPut]
		public async Task<TagHistory> Write(
			[FromBody] ValueRequest request)
		{
			Tag tag = await historyService.Db.Tags
				.Where(x => request.TagId.HasValue && x.Id == request.TagId
					|| !string.IsNullOrEmpty(request.TagName) && x.Name == request.TagName)
				.FirstOrDefaultAsync()
				?? throw new Exception(request.TagId.HasValue ? $"Тег #{request.TagId} не найден" : $"Тег \"{request.TagName}\" не найден");

			var record = tag.ToHistory(request.Value, (ushort)(request.TagQuality ?? TagQuality.Unknown));
			record.Date = request.Date;

			if (tag.SourceId == (int)CustomSource.Manual)
			{
				await historyService.WriteManualHistoryValueAsync(record);
			}
			else if (tag.SourceId == (int)CustomSource.Calculated)
			{
				throw new Exception("Запись в вычисляемые теги не поддерживается");
			}
			else
			{
				await historyService.WriteHistoryValueAsync(record);
			}

			return record;
		}
	}
}
