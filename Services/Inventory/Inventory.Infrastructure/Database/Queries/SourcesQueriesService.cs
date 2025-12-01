using Datalake.Contracts.Models.Sources;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Queries;
using LinqToDB;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class SourcesQueriesService(InventoryDbLinqContext context) : ISourcesQueriesService
{
	public async Task<List<SourceWithSettingsInfo>> GetAllAsync(bool withCustom = false, CancellationToken ct = default)
	{
		var query =
			from source in context.Sources
			where withCustom || !Source.InternalSources.Contains(source.Type)
			select new SourceWithSettingsInfo
			{
				Id = source.Id,
				Name = source.Name,
				Address = source.Address,
				Description = source.Description,
				Type = source.Type,
				IsDisabled = source.IsDisabled,
			};

		return await query.ToListAsync(ct);
	}

	public async Task<SourceWithSettingsInfo?> GetByIdAsync(int sourceId, CancellationToken ct)
	{
		var query =
			from source in context.Sources
			where source.Id == sourceId
			select new SourceWithSettingsInfo
			{
				Id = source.Id,
				Name = source.Name,
				Address = source.Address,
				Description = source.Description,
				Type = source.Type,
				IsDisabled = source.IsDisabled,
			};

		return await query.FirstOrDefaultAsync(ct);
	}

	public async Task<List<SourceTagInfo>> GetSourceTagsAsync(int sourceId, CancellationToken ct)
	{
		var query =
			from tag in context.Tags
			where tag.SourceId == sourceId && !tag.IsDeleted
			select new SourceTagInfo
			{
				Id = tag.Id,
				Name = tag.Name,
				Item = tag.SourceItem,
				Resolution = tag.Resolution,
				Type = tag.Type,
			};

		return await query.ToListAsync(ct);
	}
}
