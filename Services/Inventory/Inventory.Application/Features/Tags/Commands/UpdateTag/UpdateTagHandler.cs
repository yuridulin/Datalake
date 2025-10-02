using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Domain.Entities;
using Datalake.Shared.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.Tags.Commands.UpdateTag;

public interface IUpdateTagHandler : ICommandHandler<UpdateTagCommand, bool> { }

public class UpdateTagHandler(
	ITagsRepository tagsRepository,
	ISourcesRepository sourcesRepository,
	ITagInputsRepository tagInputsRepository,
	ITagThresholdsRepository tagThresholdsRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache,
	ILogger<UpdateTagHandler> logger) :
		TransactionalCommandHandler<UpdateTagCommand, bool>(unitOfWork, logger, inventoryCache),
		IUpdateTagHandler
{
	public override void CheckPermissions(UpdateTagCommand command)
	{
		command.User.ThrowIfNoAccessToTag(AccessType.Manager, command.Id);
	}

	TagEntity tag = null!;
	TagInputEntity[]? tagInputs;
	TagThresholdEntity[]? tagThresholds;

	public override async Task<bool> ExecuteInTransactionAsync(UpdateTagCommand command, CancellationToken ct = default)
	{
		tag = await tagsRepository.GetByIdAsync(command.Id, ct)
			?? throw InventoryNotFoundException.NotFoundTag(command.Id);

		var source = await sourcesRepository.GetByIdAsync(command.SourceId, ct)
			?? throw InventoryNotFoundException.NotFoundSource(command.SourceId);

		if (command.SourceTagId.HasValue && !await tagsRepository.ExistsAsync(command.SourceTagId.Value, ct))
			throw InventoryNotFoundException.NotFoundTag(command.SourceTagId.Value, $"Идентификатор: {nameof(command.SourceTagId)} = {command.SourceTagId}");

		if (command.ThresholdSourceTagId.HasValue && !await tagsRepository.ExistsAsync(command.ThresholdSourceTagId!.Value, ct))
			throw InventoryNotFoundException.NotFoundTag(command.ThresholdSourceTagId.Value, $"Идентификатор: {nameof(command.ThresholdSourceTagId)} = {command.SourceTagId}");

		var existsInputs = await tagInputsRepository.GetByTagIdAsync(tag.Id, ct);
		await tagInputsRepository.RemoveRangeAsync(existsInputs, ct);

		if (command.FormulaInputs.Any())
		{
			if (await tagsRepository.ExistsRangeAsync(command.FormulaInputs.Select(x => x.TagId), ct))
				throw new NotFoundException("Не все теги, указанные как входные для формулы, были найдены");

			tagInputs = command.FormulaInputs.Select(x => new TagInputEntity(tag.Id, x.TagId, x.BlockId, x.VariableName)).ToArray();
			await tagInputsRepository.AddRangeAsync(tagInputs, ct);
		}

		var existThresholds = await tagThresholdsRepository.GetByTagIdAsync(tag.Id, ct);
		await tagThresholdsRepository.RemoveRangeAsync(existThresholds, ct);

		if (command.Thresholds.Any())
		{
			tagThresholds = command.Thresholds.Select(x => new TagThresholdEntity(tag.Id, x.InputValue, x.OutputValue)).ToArray();
			await tagThresholdsRepository.AddRangeAsync(tagThresholds, ct);
		}

		tag.Update(
			name: command.Name,
			description: command.Description,
			type: command.Type,
			resolution: command.Resolution,
			sourceId: source.Id,
			sourceType: source.Type,
			sourceItem: command.SourceItem,
			isScaling: command.IsScaling,
			minEu: command.MinEu,
			maxEu: command.MaxEu,
			minRaw: command.MinRaw,
			maxRaw: command.MaxRaw,
			formula: command.Formula,
			aggregation: command.Aggregation,
			aggregationPeriod: command.AggregationPeriod,
			aggTagId: command.SourceTagId,
			aggBlockId: command.SourceTagBlockId,
			thresholdTagId: command.ThresholdSourceTagId,
			thresholdBlockId: command.ThresholdSourceTagBlockId);

		await tagsRepository.UpdateAsync(tag, ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Изменен тег: diff", tagId: tag.Id), ct);

		return true;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state)
	{
		var newState = state.WithTag(tag);

		if (tagInputs != null)
			newState = newState.WithTagInputs(tag.Id, tagInputs);

		if (tagThresholds != null)
			newState = newState.WithTagThresholds(tag.Id, tagThresholds);

		return newState;
	}
}
