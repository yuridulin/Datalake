using DatalakeDatabase.ApiModels.Tags;
using DatalakeDatabase.Enums;
using LinqToDB;

namespace DatalakeDatabase.Repositories;

public partial class TagsRepository
{
	public IQueryable<TagInfo> GetInfoWithSources()
	{
		var query = from tag in db.Tags
								from source in db.Sources.LeftJoin(x => x.Id == tag.SourceId)
								select new TagInfo
								{
									Id = tag.Id,
									Name = tag.Name,
									Description = tag.Description,
									IntervalInSeconds = tag.Interval,
									Type = tag.Type,
									Formula = tag.Formula ?? string.Empty,
									FormulaInputs = db.TagInputs
										.Where(x => x.TagId == tag.Id)
										.ToDictionary(x => x.VariableName, x => x.InputTagId),
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

		return query;
	}

	public IQueryable<TagAsInputInfo> GetPossibleInputs()
	{
		var query = db.Tags
			.Select(x => new TagAsInputInfo
			{
				Id = x.Id,
				Name = x.Name,
				Type = x.Type,
			})
			.OrderBy(x => x.Name);

		return query;
	}
}
