using Datalake.Contracts.Public.Enums;
using Datalake.DataService.Abstractions;
using Datalake.DataService.Database.Entities;
using Datalake.DataService.Database.Interfaces;
using Datalake.DataService.Extensions;
using Datalake.DataService.Models;
using Datalake.PrivateApi.Attributes;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Values;

namespace Datalake.DataService.Services.Values;

[Singleton]
public class GetValuesService(
	ITagsStore tagsStore,
	ICurrentValuesStore currentValuesStore,
	IServiceScopeFactory serviceScopeFactory) : IGetValuesService
{
	public async Task<List<ValuesResponse>> GetAsync(UserAccessEntity user, ValuesRequest[] requests)
	{
		var trustedRequests = CheckRequests(user, requests);
		var responses = await GetResponsesAsync(trustedRequests);

		return responses;
	}

	private List<ValuesTrustedRequest> CheckRequests(UserAccessEntity user, ValuesRequest[] requests)
	{
		List<ValuesTrustedRequest> trustedRequests = new(requests.Length);

		foreach (var request in requests)
		{
			List<ValuesTrustedRequest.TagSettings> tags = new(request.TagsId?.Length ?? 0 + request.Tags?.Length ?? 0);

			if (request.TagsId?.Length > 0)
			{
				foreach (var id in request.TagsId)
				{
					var tag = tagsStore.TryGet(id);

					if (tag != null)
					{
						tags.Add(new()
						{
							Guid = tag.Guid,
							Id = tag.Id,
							Name = tag.Name,
							Resolution = tag.Resolution,
							ScalingCoefficient = tag.ScalingCoefficient,
							SourceId = tag.SourceId,
							SourceType = tag.SourceType,
							Type = tag.Type,
							IsDeleted = tag.IsDeleted,
							Result = !user.HasAccessToTag(AccessType.Viewer, tag.Id)
								? ValueResult.NoAccess
								: tag.IsDeleted ? ValueResult.IsDeleted : ValueResult.Ok,
						});
					}
					else
					{
						tags.Add(new()
						{
							Id = id,
							Guid = Guid.Empty,
							Name = string.Empty,
							Resolution = TagResolution.NotSet,
							ScalingCoefficient = 1,
							SourceId = 0,
							SourceType = SourceType.NotSet,
							Type = TagType.String,
							IsDeleted = false,
							Result = ValueResult.NotFound
						});
					}
				}
			}

			if (request.Tags?.Length > 0)
			{
				foreach (var guid in request.Tags)
				{
					var tag = tagsStore.TryGet(guid);

					if (tag != null)
					{
						tags.Add(new()
						{
							Guid = tag.Guid,
							Id = tag.Id,
							Name = tag.Name,
							Resolution = tag.Resolution,
							ScalingCoefficient = tag.ScalingCoefficient,
							SourceId = tag.SourceId,
							SourceType = tag.SourceType,
							Type = tag.Type,
							IsDeleted = tag.IsDeleted,
							Result = !user.HasAccessToTag(AccessType.Viewer, tag.Id)
								? ValueResult.NoAccess
								: tag.IsDeleted ? ValueResult.IsDeleted : ValueResult.Ok,
						});
					}
					else
					{
						tags.Add(new()
						{
							Id = 0,
							Guid = guid,
							Name = string.Empty,
							Resolution = TagResolution.NotSet,
							ScalingCoefficient = 1,
							SourceId = 0,
							SourceType = SourceType.NotSet,
							Type = TagType.String,
							IsDeleted = false,
							Result = ValueResult.NotFound
						});
					}
				}
			}

			trustedRequests.Add(new()
			{
				RequestKey = request.RequestKey,
				Time = new ValuesTrustedRequest.TimeSettings
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
		var getHistoryRepository = scope.ServiceProvider.GetRequiredService<IGetHistoryRepository>();

		foreach (var sqlScope in sqlScopes)
		{
			IEnumerable<TagHistory> databaseValues;

			// получение среза
			if (!sqlScope.Settings.Old.HasValue && !sqlScope.Settings.Young.HasValue)
			{
				Dictionary<int, TagHistory?> databaseValuesById;

				if (sqlScope.Settings.Exact.HasValue)
				{
					databaseValues = await getHistoryRepository.GetExactValuesAsync(sqlScope.Settings.Exact.Value, sqlScope.TagsId);
					databaseValuesById = databaseValues.ToDictionary(x => x.TagId)!;
				}
				else
				{
					// Если не указывается ни одна дата, выполняется получение текущих значений. Не убирать!
					databaseValuesById = currentValuesStore.GetByIdentifiers(sqlScope.TagsId);
					sqlScope.Settings.Exact = DateFormats.GetCurrentDateTime();
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

				databaseValues = await getHistoryRepository.GetRangeValuesAsync(sqlScope.Settings.Old.Value, sqlScope.Settings.Young.Value, sqlScope.TagsId);

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
