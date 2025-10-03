using Datalake.Contracts.Public.Enums;
using Datalake.DataService.Abstractions;
using Datalake.DataService.Database.Entities;
using Datalake.DataService.Database.Interfaces;
using Datalake.Shared.Application;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.Values;

namespace Datalake.DataService.Services.Values;

[Singleton]
public class ManualWriteValuesService(
	ITagsStore tagsStore,
	ICurrentValuesStore currentValuesStore,
	IServiceScopeFactory serviceScopeFactory,
	ITagHistoryFactory tagHistoryFactory) : IManualWriteValuesService
{
	public async Task<List<ValuesTagResponse>> WriteAsync(
		UserAccessEntity user,
		ValueWriteRequest[] requests)
	{
		List<ValuesTagResponse> responses = [];
		List<TagHistory> recordsToWrite = [];

		foreach (var request in requests)
		{
			TagCacheInfo? tag = null;
			request.Date ??= DateFormats.GetCurrentDateTime();

			if (request.Id.HasValue)
				tag = tagsStore.TryGet(request.Id.Value);
			else if (request.Guid.HasValue)
				tag = tagsStore.TryGet(request.Guid.Value);

			if (tag == null)
			{
				responses.Add(new ValuesTagResponse
				{
					Result = ValueResult.NotFound,
					Id = request.Id ?? 0,
					Guid = request.Guid ?? Guid.Empty,
					Name = string.Empty,
					Type = TagType.String,
					Resolution = TagResolution.NotSet,
					SourceType = SourceType.NotSet,
					Values = [
						new ValueRecord
						{
							Date = request.Date.Value,
							DateString = request.Date.Value.ToString(DateFormats.HierarchicalWithMilliseconds),
							Quality = TagQuality.Unknown,
							Value = null,
						}
					]
				});
				continue;
			}

			var record = tagHistoryFactory.CreateFrom(tag, request);

			var result =
				tag.IsDeleted ? ValueResult.IsDeleted
				: tag.SourceType != SourceType.Manual ? ValueResult.NotManual
				: !user.HasAccessToTag(AccessType.Editor, tag.Id) ? ValueResult.NoAccess
				: ValueResult.Ok;

			responses.Add(new ValuesTagResponse
			{
				Result = result,
				Id = tag.Id,
				Guid = tag.Guid,
				Name = tag.Name,
				Type = tag.Type,
				Resolution = tag.Resolution,
				SourceType = tag.SourceType,
				Values = [
					new ValueRecord
					{
						Date = record.Date,
						DateString = record.Date.ToString(DateFormats.HierarchicalWithMilliseconds),
						Quality = record.Quality,
						Value = record.GetTypedValue(tag.Type),
					}
				]
			});

			if (result == ValueResult.Ok)
				recordsToWrite.Add(record);
		}

		var uniqueRecords = recordsToWrite
			.GroupBy(x => new { x.TagId, x.Date })
			.Select(g => g.First())
			.ToList();

		using var scope = serviceScopeFactory.CreateScope();
		var writeHistoryRepository = scope.ServiceProvider.GetRequiredService<IWriteHistoryRepository>();

		var writeResult = await writeHistoryRepository.WriteAsync(uniqueRecords);
		if (writeResult)
		{
			// обновление в кэше текущих данных
			foreach (var record in uniqueRecords)
				currentValuesStore.TryUpdate(record.TagId, record);
		}
		else
		{
			foreach (var response in responses)
			{
				if (response.Result == ValueResult.Ok)
					response.Result = ValueResult.UnknownError;
			}
		}

		return responses;
	}
}
