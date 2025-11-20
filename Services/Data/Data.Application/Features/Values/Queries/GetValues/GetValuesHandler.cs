using Datalake.Contracts.Models.Data.Values;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Data.Application.Interfaces.Storage;
using Datalake.Data.Application.Models.Tags;
using Datalake.Data.Application.Models.Values;
using Datalake.Domain.Entities;
using Datalake.Domain.Enums;
using Datalake.Domain.Extensions;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Datalake.Data.Application.Features.Values.Queries.GetValues;

public interface IGetValuesHandler : IQueryHandler<GetValuesQuery, IEnumerable<ValuesResponse>> { }

public class GetValuesHandler(
	ITagsSettingsStore tagsStore,
	ITagsUsageStore tagsUsageStore,
	IValuesStore currentValuesStore,
	IServiceScopeFactory serviceScopeFactory) : IGetValuesHandler
{
	public async Task<IEnumerable<ValuesResponse>> HandleAsync(GetValuesQuery query, CancellationToken ct = default)
	{
		var trustedRequests = CheckRequests(query.User, query.Requests.ToArray());
		var responses = await GetResponsesAsync(trustedRequests);

		return responses;
	}

	private List<ValuesTrustedRequest> CheckRequests(UserAccessValue user, ValuesRequest[] requests)
	{
		List<ValuesTrustedRequest> trustedRequests = new(requests.Length);

		foreach (var request in requests)
		{
			List<TagSettingsResponse> tags = new(request.TagsId?.Length ?? 0 + request.Tags?.Length ?? 0);

			if (request.TagsId?.Length > 0)
			{
				foreach (var id in request.TagsId)
				{
					var tag = tagsStore.TryGet(id);

					if (tag != null)
					{
						tags.Add(new()
						{
							TagId = tag.TagId,
							TagGuid = tag.TagGuid,
							TagName = tag.TagName,
							TagResolution = tag.TagResolution,
							TagType = tag.TagType,
							SourceId = tag.SourceId,
							SourceType = tag.SourceType,
							Result = !user.HasAccessToTag(AccessType.Viewer, tag.TagId)
								? ValueResult.NoAccess
								: tag.IsDeleted ? ValueResult.IsDeleted : ValueResult.Ok,
						});

						tagsUsageStore.RegisterUsage(tag.TagId, request.RequestKey);
					}
					else
					{
						tags.Add(new()
						{
							TagId = id,
							TagGuid = Guid.Empty,
							TagName = string.Empty,
							TagType = TagType.String,
							TagResolution = TagResolution.None,
							SourceId = 0,
							SourceType = SourceType.Unset,
							Result = ValueResult.NotFound,
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
							TagId = tag.TagId,
							TagGuid = tag.TagGuid,
							TagName = tag.TagName,
							TagResolution = tag.TagResolution,
							TagType = tag.TagType,
							SourceId = tag.SourceId,
							SourceType = tag.SourceType,
							Result = !user.HasAccessToTag(AccessType.Viewer, tag.TagId)
								? ValueResult.NoAccess
								: tag.IsDeleted ? ValueResult.IsDeleted : ValueResult.Ok,
						});

						tagsUsageStore.RegisterUsage(tag.TagId, request.RequestKey);
					}
					else
					{
						tags.Add(new()
						{
							TagId = 0,
							TagGuid = guid,
							TagName = string.Empty,
							TagType = TagType.String,
							TagResolution = TagResolution.None,
							SourceId = 0,
							SourceType = SourceType.Unset,
							Result = ValueResult.NotFound,
						});
					}
				}
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
				TagsId = g.SelectMany(r => r.Tags).Where(r => r.Result == ValueResult.Ok).Select(r => r.TagId).ToArray(),
			})
			.ToArray();

		using var scope = serviceScopeFactory.CreateScope();
		var tagsHistoryRepository = scope.ServiceProvider.GetRequiredService<ITagsValuesRepository>();

		foreach (var sqlScope in sqlScopes)
		{
			IEnumerable<TagValue> databaseValues;

			// получение среза
			if (!sqlScope.Settings.Old.HasValue && !sqlScope.Settings.Young.HasValue)
			{
				Dictionary<int, TagValue?> databaseValuesById;

				if (sqlScope.Settings.Exact.HasValue)
				{
					databaseValues = await tagsHistoryRepository.GetExactAsync(sqlScope.TagsId, sqlScope.Settings.Exact.Value);
					databaseValuesById = databaseValues.ToDictionary(x => x.TagId)!;
				}
				else
				{
					// Если не указывается ни одна дата, выполняется получение текущих значений. Не убирать!
					databaseValuesById = currentValuesStore.GetByIdentifiers(sqlScope.TagsId);
					sqlScope.Settings.Exact = DateTime.UtcNow;
				}

				foreach (var request in sqlScope.Requests)
				{
					List<ValuesTagResponse> tags = [];
					foreach (var tag in request.Tags)
					{
						ValuesTagResponse tagResponse = new()
						{
							Result = tag.Result,
							Id = tag.TagId,
							Guid = tag.TagGuid,
							Name = tag.TagName,
							Type = tag.TagType,
							Resolution = tag.TagResolution,
							SourceType = tag.SourceType,
							Values = [],
						};

						if (tag.Result == ValueResult.Ok)
						{
							if (!databaseValuesById.TryGetValue(tag.TagId, out var value) || value == null)
							{
								tag.Result = ValueResult.ValueNotFound;
								value = TagValue.AsEmpty(tag.TagId, sqlScope.Settings.Exact.Value, TagQuality.Bad_NoValues);
							}

							var tagValue = new ValueRecord
							{
								Date = value.Date,
								Quality = value.Quality,
								Text = value.Text,
								Number = value.Number,
								Boolean = value.Boolean,
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
				sqlScope.Settings.Young ??= DateTime.UtcNow;
				sqlScope.Settings.Old ??= sqlScope.Settings.Young;

				databaseValues = await tagsHistoryRepository.GetRangeAsync(sqlScope.TagsId, sqlScope.Settings.Old.Value, sqlScope.Settings.Young.Value);

				foreach (var request in sqlScope.Requests)
				{
					var response = new ValuesResponse
					{
						RequestKey = request.RequestKey,
						Tags = [],
					};

					var tagsResponses = new List<ValuesTagResponse>();
					var requestIdentifiers = request.Tags.Select(t => t.TagId).ToArray();
					var requestValues = databaseValues.Where(x => requestIdentifiers.Contains(x.TagId));

					foreach (var tag in request.Tags)
					{
						var tagResponse = new ValuesTagResponse
						{
							Result = tag.Result,
							Guid = tag.TagGuid,
							Id = tag.TagId,
							Name = tag.TagName,
							Type = tag.TagType,
							Resolution = tag.TagResolution,
							SourceType = tag.SourceType,
							Values = [],
						};
						var tagValues = requestValues.Where(x => x.TagId == tag.TagId).ToList();

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
										Quality = TagQuality.Bad_NoValues,
										Text = null,
										Number = null,
										Boolean = null,
									}
								];
							}
							if (request.Resolution != null && request.Resolution > 0)
							{
								tagValues = StretchByResolution(tagValues, sqlScope.Settings.Old.Value, sqlScope.Settings.Young.Value, request.Resolution.Value);
							}

							if (tag.TagType == TagType.Number && request.Func != TagAggregation.None)
							{
								var numericValues = tagValues
									.Where(x => x.Quality == TagQuality.Good || x.Quality == TagQuality.Good_ManualWrite)
									.Select(x => x.Number);

								float? value = 0;
								try
								{
									switch (request.Func)
									{
										case TagAggregation.Sum:
											value = numericValues.Sum();
											break;

										case TagAggregation.Average:
											value = numericValues.Average();
											break;

										case TagAggregation.Min:
											value = numericValues.Min();
											break;

										case TagAggregation.Max:
											value = numericValues.Max();
											break;
									}

									tagResponse.Values = [
										new() {
											Date = sqlScope.Settings.Old.Value,
											Quality = TagQuality.Good,
											Text = null,
											Number = value,
											Boolean = null,
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
										Quality = x.Quality,
										Text = x.Text,
										Number = x.Number,
										Boolean = x.Boolean,
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

	private static List<TagValue> StretchByResolution(
		List<TagValue> valuesByChange,
		DateTime old,
		DateTime young,
		TagResolution resolution)
	{
		var timeRange = (young - old).TotalMilliseconds;
		List<TagValue> continuous = [];
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
