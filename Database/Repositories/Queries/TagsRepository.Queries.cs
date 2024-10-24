using Datalake.Database.Enums;
using Datalake.Database.Models.Tags;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class TagsRepository
{
	public IQueryable<TagInfo> GetInfoWithSources(Guid? energoId = null)
	{
		// TODO: energoId
		if (energoId.HasValue)
		{ }

		var inputs =
			from input_rel in db.TagInputs
			from input in db.Tags.InnerJoin(x => x.Id == input_rel.InputTagId)
			select new { input, input_rel };

#pragma warning disable IDE0305 // Упростите инициализацию коллекции
		var query =
			from tag in db.Tags
			from source in db.Sources.LeftJoin(x => x.Id == tag.SourceId)
			select new TagInfo
			{
				Id = tag.Id,
				Guid = tag.GlobalGuid,
				Name = tag.Name,
				Description = tag.Description,
				IntervalInSeconds = tag.Interval,
				Type = tag.Type,
				Formula = tag.Formula ?? string.Empty,
				FormulaInputs = inputs
					.Where(x => x.input_rel.TagId == tag.Id)
					.Select(x => new TagInputInfo
					{
						Id = x.input.Id,
						Guid = x.input.GlobalGuid,
						Name = x.input.Name,
						VariableName = x.input_rel.VariableName,
					})
					.ToArray(),
				IsScaling = tag.IsScaling,
				MaxEu = tag.MaxEu,
				MaxRaw = tag.MaxRaw,
				MinEu = tag.MinEu,
				MinRaw = tag.MinRaw,
				SourceId = tag.SourceId,
				SourceItem = tag.SourceItem,
				SourceType = source != null ? source.Type : SourceType.Custom,
				SourceName = source != null ? source.Name : "Unknown",
			};
#pragma warning restore IDE0305 // Упростите инициализацию коллекции

		return query;
	}

	public IQueryable<TagAsInputInfo> GetPossibleInputs()
	{
		var query = db.Tags
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
		var query =
			from t in db.Tags
			from s in db.Sources.LeftJoin(x => x.Id == t.SourceId)
			select new TagCacheInfo
			{
				Id = t.Id,
				Guid = t.GlobalGuid,
				Name = t.Name,
				TagType = t.Type,
				SourceType = s.Type,
				IsManual = t.SourceId == (int)CustomSource.Manual,
				ScalingCoefficient = t.IsScaling
					? ((t.MaxEu - t.MinEu) / (t.MaxRaw - t.MinRaw))
					: 1,
			};

		return query;
	}
}
