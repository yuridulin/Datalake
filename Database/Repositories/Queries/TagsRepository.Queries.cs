using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.Tags;
using LinqToDB;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Database.Repositories;

public partial class TagsRepository
{
	public IQueryable<TagInfo> GetInfoWithSources()
	{
		var query = db.Tags
			.Include(x => x.TagInputs)
			.ThenInclude(x => x.Tag)
			.Include(x => x.Source)
			.AsNoTracking()
			.Select(x => new TagInfo
			{
				Id = x.Id,
				Guid = x.GlobalGuid,
				Name = x.Name,
				Description = x.Description,
				IntervalInSeconds = x.Interval,
				Type = x.Type,
				Formula = x.Formula ?? string.Empty,
				FormulaInputs = x.TagInputs
					.Where(x => x.InputTag != null)
					.Select(x => new TagInputInfo
					{
						Id = x.InputTag!.Id,
						Guid = x.InputTag!.GlobalGuid,
						Name = x.InputTag!.Name,
						VariableName = x.VariableName,
					})
					.ToArray(),
				IsScaling = x.IsScaling,
				MaxEu = x.MaxEu,
				MaxRaw = x.MaxRaw,
				MinEu = x.MinEu,
				MinRaw = x.MinRaw,
				SourceId = x.SourceId,
				SourceItem = x.SourceItem,
				SourceType = x.Source != null ? x.Source.Type : SourceType.Custom,
				SourceName = x.Source != null ? x.Source.Name : "Unknown",
			});

		return query;
	}

	public IQueryable<TagAsInputInfo> GetPossibleInputs()
	{
		var query = db.Tags
			.AsNoTracking()
			.Select(x => new TagAsInputInfo
			{
				Id = x.Id,
				Guid = x.GlobalGuid,
				Name = x.Name,
				Type = x.Type,
			})
			.OrderBy(x => x.Name);

		return query;
	}

	public IQueryable<TagCacheInfo> GetTagsForCache()
	{
		var query = db.Tags
			.Include(x => x.Source)
			.AsNoTracking()
			.Select(x => new TagCacheInfo
			{
				Id = x.Id,
				Guid = x.GlobalGuid,
				Name = x.Name,
				TagType = x.Type,
				SourceType = x.Source != null ? x.Source.Type : SourceType.Custom,
				IsManual = x.SourceId == (int)CustomSource.Manual,
				ScalingCoefficient = x.IsScaling
					? ((x.MaxEu - x.MinEu) / (x.MaxRaw - x.MinRaw))
					: 1,
			});

		return query;
	}
}
