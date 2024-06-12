using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.Tags;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class TagsRepository
{
	public IQueryable<TagInfo> GetInfoWithSources()
	{
		var query = from tag in db.Tags
								from source in db.Sources.LeftJoin(x => x.Id == tag.SourceId)
								select new TagInfo
								{
									Guid = tag.GlobalGuid,
									Name = tag.Name,
									Description = tag.Description,
									IntervalInSeconds = tag.Interval,
									Type = tag.Type,
									Formula = tag.Formula ?? string.Empty,
									FormulaInputs = db.TagInputs
										.Where(x => x.TagId == tag.Id)
										.Select(x => new TagInputInfo
										{
											Id = x.InputTagId,
											Name = x.VariableName,
											VariableName = x.VariableName,
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
