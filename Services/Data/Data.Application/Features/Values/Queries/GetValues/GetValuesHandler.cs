using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Data.Api.Enums;
using Datalake.Data.Api.Models.Values;
using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Data.Application.Models.Tags;
using Datalake.Data.Application.Models.Values;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Datalake.Data.Application.Features.Values.Queries.GetValues;

public interface IGetValuesHandler : IQueryHandler<GetValuesQuery, IEnumerable<ValuesResponse>> { }

public class GetValuesHandler(
		ITagsStore tagsStore,
		ICurrentValuesStore currentValuesStore,
		IServiceScopeFactory serviceScopeFactory) : IGetValuesHandler
{
	public async Task<IEnumerable<ValuesResponse>> HandleAsync(GetValuesQuery query, CancellationToken ct = default)
	{
		// 1. Собираем все уникальные ID тегов из всех запросов
		var allTagIds = query.Requests
				.SelectMany(r => r.TagsId)
				.Distinct()
				.ToArray();

		// 2. Проверяем существование и доступ для всех тегов
		var tagAccessInfo = CheckTagAccess(query.User, allTagIds);

		// 3. Группируем запросы по временным параметрам
		var timeGroups = GroupRequestsByTime(query.Requests, tagAccessInfo);

		// 4. Выполняем запросы к БД для каждой группы
		var timeGroupResults = await ExecuteTimeGroupQueriesAsync(timeGroups, ct);

		// 5. Собираем финальные ответы
		return BuildResponses(query.Requests, tagAccessInfo, timeGroupResults);
	}

	private Dictionary<int, TagAccessInfo> CheckTagAccess(UserAccessEntity user, int[] tagIds)
	{
		var result = new Dictionary<int, TagAccessInfo>();

		foreach (var tagId in tagIds)
		{
			var tag = tagsStore.TryGet(tagId);
			var accessInfo = new TagAccessInfo();

			if (tag == null)
			{
				accessInfo.Result = ValueResult.NotFound;
			}
			else if (!user.HasAccessToTag(AccessType.Viewer, tagId))
			{
				accessInfo.Result = ValueResult.NoAccess;
				accessInfo.Tag = tag; // Сохраняем информацию о теге для ответа
			}
			else
			{
				accessInfo.Result = ValueResult.Ok;
				accessInfo.Tag = tag;
			}

			result[tagId] = accessInfo;
		}

		return result;
	}

	private List<TimeGroup> GroupRequestsByTime(IEnumerable<ValuesRequest> requests, Dictionary<int, TagAccessInfo> tagAccessInfo)
	{
		var groups = new Dictionary<TimeGroupKey, TimeGroup>();

		foreach (var request in requests)
		{
			var key = new TimeGroupKey
			{
				Old = request.Old,
				Young = request.Young,
				Exact = request.Exact
			};

			if (!groups.TryGetValue(key, out var group))
			{
				group = new TimeGroup { Key = key };
				groups[key] = group;
			}

			// Добавляем только теги с доступом для этого запроса
			var accessibleTagIds = request.TagsId
					.Where(tagId => tagAccessInfo[tagId].Result == ValueResult.Ok)
					.ToArray();

			group.Requests.Add(new TimeGroupRequest
			{
				OriginalRequest = request,
				AccessibleTagIds = accessibleTagIds
			});

			// Собираем все уникальные ID тегов для этой группы
			group.AllTagIds = group.AllTagIds.Union(accessibleTagIds).ToArray();
		}

		return groups.Values.ToList();
	}

	private async Task<Dictionary<TimeGroupKey, TimeGroupResult>> ExecuteTimeGroupQueriesAsync(
			List<TimeGroup> timeGroups, CancellationToken ct)
	{
		var results = new Dictionary<TimeGroupKey, TimeGroupResult>();

		using var scope = serviceScopeFactory.CreateScope();
		var tagsHistoryRepository = scope.ServiceProvider.GetRequiredService<ITagsHistoryRepository>();

		foreach (var group in timeGroups)
		{
			var groupResult = new TimeGroupResult();
			var tagIds = group.AllTagIds;

			if (tagIds.Length == 0)
			{
				results[group.Key] = groupResult;
				continue;
			}

			// Определяем тип запроса по времени
			if (group.Key.Exact.HasValue)
			{
				// Точечный запрос
				var values = await tagsHistoryRepository.GetExactAsync(tagIds, group.Key.Exact.Value);
				groupResult.ValuesByTagId = values.ToDictionary(x => x.TagId, x => new[] { x });
			}
			else if (!group.Key.Old.HasValue && !group.Key.Young.HasValue)
			{
				// Текущие значения
				var currentValues = currentValuesStore.GetByIdentifiers(tagIds);
				groupResult.ValuesByTagId = currentValues
						.Where(kv => kv.Value != null)
						.ToDictionary(kv => kv.Key, kv => new[] { kv.Value });
			}
			else
			{
				// Диапазон
				var from = group.Key.Old ?? DateTime.MinValue;
				var to = group.Key.Young ?? DateTime.MaxValue;
				var values = await tagsHistoryRepository.GetRangeAsync(tagIds, from, to);
				groupResult.ValuesByTagId = values.GroupBy(x => x.TagId).ToDictionary(g => g.Key, g => g.ToArray());
			}

			results[group.Key] = groupResult;
		}

		return results;
	}

	private List<ValuesResponse> BuildResponses(
			IEnumerable<ValuesRequest> requests,
			Dictionary<int, TagAccessInfo> tagAccessInfo,
			Dictionary<TimeGroupKey, TimeGroupResult> timeGroupResults)
	{
		var responses = new List<ValuesResponse>();

		foreach (var request in requests)
		{
			var response = new ValuesResponse
			{
				RequestKey = request.RequestKey,
				Tags = new List<ValuesTagResponse>()
			};

			var timeKey = new TimeGroupKey
			{
				Old = request.Old,
				Young = request.Young,
				Exact = request.Exact
			};

			var timeGroupResult = timeGroupResults[timeKey];

			foreach (var tagId in request.TagsId)
			{
				var accessInfo = tagAccessInfo[tagId];
				var tagResponse = CreateTagResponse(accessInfo, request, timeGroupResult);
				response.Tags.Add(tagResponse);
			}

			responses.Add(response);
		}

		return responses;
	}

	private ValuesTagResponse CreateTagResponse(
			TagAccessInfo accessInfo,
			ValuesRequest request,
			TimeGroupResult timeGroupResult)
	{
		var tagResponse = new ValuesTagResponse
		{
			Result = accessInfo.Result
		};

		if (accessInfo.Tag != null)
		{
			tagResponse.Id = accessInfo.Tag.Id;
			tagResponse.Guid = accessInfo.Tag.Guid;
			tagResponse.Name = accessInfo.Tag.Name;
			tagResponse.Type = accessInfo.Tag.Type;
			tagResponse.Resolution = accessInfo.Tag.Resolution;
			tagResponse.SourceType = accessInfo.Tag.SourceType;
		}

		if (accessInfo.Result != ValueResult.Ok)
		{
			// Заглушка для тегов с ошибками
			tagResponse.Values = new[]
			{
								CreateStubValue(DateTime.UtcNow, TagQuality.Bad_NoValues)
						};
		}
		else
		{
			// Получаем значения из результатов группы
			var values = timeGroupResult.ValuesByTagId.GetValueOrDefault(accessInfo.Tag.Id) ?? Array.Empty<TagHistory>();
			tagResponse.Values = ProcessTagValues(values, accessInfo.Tag, request);
		}

		return tagResponse;
	}

	private ValueRecord[] ProcessTagValues(
			TagHistory[] values,
			TagSettingsDto tag,
			ValuesRequest request)
	{
		if (values.Length == 0)
		{
			return new[] { CreateStubValue(DateTime.UtcNow, TagQuality.Bad_NoValues) };
		}

		// Здесь добавляем логику для Resolution и AggregationFunc
		// (аналогично вашему исходному коду, но упрощенно)

		if (request.Resolution.HasValue && request.Resolution.Value > 0)
		{
			values = StretchByResolution(values, request.Old, request.Young, request.Resolution.Value);
		}

		if (tag.Type == TagType.Number && request.Func != AggregationFunc.List)
		{
			// Применяем агрегацию
			var aggregatedValue = ApplyAggregation(values, request.Func);
			return new[] { aggregatedValue };
		}

		return values.Select(v => new ValueRecord
		{
			Date = v.Date,
			Text = v.Text,
			Number = v.Number,
			Boolean = v.Boolean,
			Quality = v.Quality
		}).ToArray();
	}

	private ValueRecord CreateStubValue(DateTime date, TagQuality quality)
	{
		return new ValueRecord
		{
			Date = date,
			Text = null,
			Number = null,
			Boolean = null,
			Quality = quality
		};
	}

	// Дополнительные вспомогательные классы
	private class TagAccessInfo
	{
		public ValueResult Result { get; set; }
		public TagSettingsDto Tag { get; set; }
	}

	private class TimeGroupKey
	{
		public DateTime? Old { get; set; }
		public DateTime? Young { get; set; }
		public DateTime? Exact { get; set; }

		public override bool Equals(object obj) => obj is TimeGroupKey other &&
				Old == other.Old && Young == other.Young && Exact == other.Exact;

		public override int GetHashCode() => HashCode.Combine(Old, Young, Exact);
	}

	private class TimeGroup
	{
		public TimeGroupKey Key { get; set; }
		public List<TimeGroupRequest> Requests { get; set; } = new();
		public int[] AllTagIds { get; set; } = Array.Empty<int>();
	}

	private class TimeGroupRequest
	{
		public ValuesRequest OriginalRequest { get; set; }
		public int[] AccessibleTagIds { get; set; }
	}

	private class TimeGroupResult
	{
		public Dictionary<int, TagHistory[]> ValuesByTagId { get; set; } = new();
	}

	// Остаются методы StretchByResolution, ApplyAggregation и другие вспомогательные
	// из вашего исходного кода
}

public class GetValuesHandler(
	ITagsStore tagsStore,
	ICurrentValuesStore currentValuesStore,
	IServiceScopeFactory serviceScopeFactory) : IGetValuesHandler
{
	public async Task<IEnumerable<ValuesResponse>> HandleAsync(GetValuesQuery query, CancellationToken ct = default)
	{
		List<ValuesTrustedRequest> trustedRequests = [];

		foreach (var request in query.Requests)
		{
			
		}
	}

	private List<ValuesTrustedRequest> CheckRequests(UserAccessEntity user, IEnumerable<ValuesRequest> requests)
	{
		List<ValuesTrustedRequest> trustedRequests = new(requests.Count());

		foreach (var request in requests)
		{
			List<TagSettingsDto> tags = new(request.TagsId.Length);

			foreach (var id in request.TagsId)
			{
				var tag = tagsStore.TryGet(id);
				tags.Add(tag);
			}

			trustedRequests.Add(new()
			{
				RequestKey = request.RequestKey,
				Time = new TimeSettings
				{
					Old = request.Old,
					Young = request.Young,
					Exact = request.Exact,
				},
				Resolution = request.Resolution,
				Func = request.Func,
				Tags = tags.ToArray(),
			});
		}

		return trustedRequests;
	}

	private async Task<List<ValuesResponse>> GetResponsesAsync(List<ValuesTrustedRequest> trustedRequests)
	{
		List<ValuesResponse> responses = [];

		var sqlScopes = trustedRequests
			.GroupBy(x => x.Time)
			.Select(g => new ValuesSqlScope
			{
				Settings = g.Key,
				Requests = g.ToArray(),
				Keys = g.Select(x => x.RequestKey).ToArray(),
				TagsId = g.SelectMany(r => r.Tags).Where(r => r.Result == ValueResult.Ok).Select(r => r.Id).ToArray(),
			})
			.ToArray();

		using var scope = serviceScopeFactory.CreateScope();
		var tagsHistoryRepository = scope.ServiceProvider.GetRequiredService<ITagsHistoryRepository>();

		foreach (var sqlScope in sqlScopes)
		{
			IEnumerable<TagHistory> databaseValues;

			// получение среза
			if (!sqlScope.Settings.Old.HasValue && !sqlScope.Settings.Young.HasValue)
			{
				Dictionary<int, TagHistory?> databaseValuesById;

				if (sqlScope.Settings.Exact.HasValue)
				{
					databaseValues = await tagsHistoryRepository.GetExactAsync(sqlScope.TagsId, sqlScope.Settings.Exact.Value);
					databaseValuesById = databaseValues.ToDictionary(x => x.TagId)!;
				}
				else
				{
					// Если не указывается ни одна дата, выполняется получение текущих значений. Не убирать!
					databaseValuesById = currentValuesStore.GetByIdentifiers(sqlScope.TagsId);
					sqlScope.Settings.Exact = DateTimeExtension.GetCurrentDateTime();
				}

				foreach (var request in sqlScope.Requests)
				{
					List<ValuesTagResponse> tags = [];
					foreach (var tag in request.Tags)
					{
						ValuesTagResponse tagResponse = new()
						{
							Result = tag.Result,
							Id = tag.Id,
							Guid = tag.Guid,
							Name = tag.Name,
							Type = tag.Type,
							Resolution = tag.Resolution,
							SourceType = tag.SourceType,
							Values = [],
						};

						if (tag.Result == ValueResult.Ok)
						{
							if (!databaseValuesById.TryGetValue(tag.Id, out var value) || value == null)
							{
								tag.Result = ValueResult.ValueNotFound;
								value = new TagHistory(tag.Id, sqlScope.Settings.Exact.Value);
							}

							var tagValue = new ValueRecord
							{
								Date = value.Date,
								DateString = value.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
								Quality = value.Quality,
								Value = value.GetTypedValue(tag.Type),
							};

							tagResponse.Values = [tagValue];
						}

						tags.Add(tagResponse);
					}

					var response = new ValuesResponse
					{
						RequestKey = request.RequestKey,
						Tags = tags,
					};

					responses.Add(response);
				}
			}

			// получение истории
			else
			{
				sqlScope.Settings.Young ??= DateFormats.GetCurrentDateTime();
				sqlScope.Settings.Old ??= sqlScope.Settings.Young;

				databaseValues = await tagsHistoryRepository.GetRangeValuesAsync(sqlScope.Settings.Old.Value, sqlScope.Settings.Young.Value, sqlScope.TagsId);

				foreach (var request in sqlScope.Requests)
				{
					var response = new ValuesResponse
					{
						RequestKey = request.RequestKey,
						Tags = [],
					};

					var tagsResponses = new List<ValuesTagResponse>();
					var requestIdentifiers = request.Tags.Select(t => t.Id).ToArray();
					var requestValues = databaseValues.Where(x => requestIdentifiers.Contains(x.TagId));

					foreach (var tag in request.Tags)
					{
						var tagResponse = new ValuesTagResponse
						{
							Result = tag.Result,
							Guid = tag.Guid,
							Id = tag.Id,
							Name = tag.Name,
							Type = tag.Type,
							Resolution = tag.Resolution,
							SourceType = tag.SourceType,
							Values = [],
						};
						var tagValues = requestValues.Where(x => x.TagId == tag.Id).ToList();

						// если у нас не Ok, то тега нет, или нет доступа к нему
						// так или иначе, значения нас уже не интересуют
						if (tagResponse.Result == ValueResult.Ok)
						{
							// так как при получении истории мы делаем locf значений до начала диапазона, у нас должно быть минимум одно значение
							// если ноль - условие не корректно или тега вообще не существовало
							if (tagValues.Count == 0)
							{
								tagResponse.Result = ValueResult.ValueNotFound;
								tagResponse.Values = [
									new()
									{
										Date = sqlScope.Settings.Old.Value,
										DateString = sqlScope.Settings.Old.Value.ToString(DateFormats.HierarchicalWithMilliseconds),
										Quality = TagQuality.Bad_NoValues,
										Value = 0,
									}
								];
							}
							if (request.Resolution != null && request.Resolution > 0)
							{
								tagValues = StretchByResolution(tagValues, sqlScope.Settings.Old.Value, sqlScope.Settings.Young.Value, request.Resolution.Value);
							}

							if (tag.Type == TagType.Number && request.Func != AggregationFunc.List)
							{
								var numericValues = tagValues
									.Where(x => x.Quality == TagQuality.Good || x.Quality == TagQuality.Good_ManualWrite)
									.Select(x => x.GetTypedValue(TagType.Number) as float?);

								float? value = 0;
								try
								{
									switch (request.Func)
									{
										case AggregationFunc.Sum:
											value = numericValues.Sum();
											break;

										case AggregationFunc.Avg:
											value = numericValues.Average();
											break;

										case AggregationFunc.Min:
											value = numericValues.Min();
											break;

										case AggregationFunc.Max:
											value = numericValues.Max();
											break;
									}

									tagResponse.Values = [
										new() {
											Date = sqlScope.Settings.Old.Value,
											DateString = sqlScope.Settings.Old.Value.ToString(DateFormats.HierarchicalWithMilliseconds),
											Quality = TagQuality.Good,
											Value = value,
										}
									];
								}
								catch
								{
								}
							}
							else
							{
								tagResponse.Values = [
									..tagValues
									.Select(x => new ValueRecord
									{
										Date = x.Date,
										DateString = x.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
										Quality = x.Quality,
										Value = x.GetTypedValue(tag.Type),
									})
									.OrderBy(x => x.Date)
								];
							}
						}

						tagsResponses.Add(tagResponse);
					}

					response.Tags = tagsResponses;
					responses.Add(response);
				}
			}
		}

		return responses;
	}


	private const int _stretchLimit = 100000;

	private static List<TagHistory> StretchByResolution(
		List<TagHistory> valuesByChange,
		DateTime old,
		DateTime young,
		TagResolution resolution)
	{
		var timeRange = (young - old).TotalMilliseconds;
		List<TagHistory> continuous = [];
		DateTime stepDate = old;
		int step = 0;

		do
		{
			var value = valuesByChange
				.Where(x => x.Date <= stepDate)
				.OrderByDescending(x => x.Date)
				.FirstOrDefault();

			if (value != null)
				continuous.Add(value.StretchToDate(stepDate));

			stepDate = stepDate.AddByResolution(resolution);
			step++;
		}
		while (stepDate <= young && step < _stretchLimit);

		return continuous;
	}
}


public record Scope
{
	public List<int> TagsId { get; set; } = [];
}
