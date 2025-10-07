using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Data.Api.Enums;
using Datalake.Data.Api.Models.Values;
using Datalake.Data.Application.Features.Values.Commands.SystemWriteValues;
using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Data.Application.Models.Tags;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Values.Commands.ManualWriteValues;

public interface IManualWriteValuesHandler : ICommandHandler<ManualWriteValuesCommand, IEnumerable<ValuesTagResponse>> { }

public class ManualWriteValuesHandler(
	ISystemWriteValuesHandler systemWriteValuesHandler,
	ICurrentValuesStore currentValuesStore,
	ITagsStore tagsStore) : IManualWriteValuesHandler
{
	public async Task<IEnumerable<ValuesTagResponse>> HandleAsync(ManualWriteValuesCommand command, CancellationToken ct = default)
	{
		List<ValuesTagResponse> responses = [];
		List<TagHistory> recordsToWrite = [];

		foreach (var request in command.Requests)
		{
			TagSettingsDto? tag = null;
			var date = request.Date ?? DateTimeExtension.GetCurrentDateTime();

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
					Resolution = TagResolution.None,
					SourceType = SourceType.Unset,
					Values = [
						new ValueRecord
						{
							Date = date,
							Quality = TagQuality.Unknown,
							Boolean = null,
							Number = null,
							Text = null,
						}
					]
				});
				continue;
			}

			var record = new TagHistory(tag.TagId, tag.TagType, request.Date, request.Quality, request.Value, tag.ScaleSettings?.GetScale());

			var result =
				tag.IsDeleted ? ValueResult.IsDeleted
				: tag.SourceType != SourceType.Manual ? ValueResult.NotManual
				: !command.User.HasAccessToTag(AccessType.Editor, tag.TagId) ? ValueResult.NoAccess
				: ValueResult.Ok;

			responses.Add(new ValuesTagResponse
			{
				Result = result,
				Id = tag.TagId,
				Guid = tag.TagGuid,
				Name = tag.TagName,
				Type = tag.TagType,
				Resolution = tag.TagResolution,
				SourceType = tag.SourceType,
				Values = [
					new ValueRecord
					{
						Date = record.Date,
						Quality = record.Quality,
						Boolean = record.Boolean,
						Number = record.Number,
						Text = record.Text,
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

		var writeResult = await systemWriteValuesHandler.HandleAsync(new() { Values = uniqueRecords }, ct);
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
