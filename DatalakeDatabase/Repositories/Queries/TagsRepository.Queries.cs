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
									CalcInfo = new TagInfo.TagCalcInfo
									{
										Formula = tag.Formula ?? string.Empty,
										Inputs = db.TagInputs
											.Where(x => x.TagId == tag.Id)
											.ToDictionary(x => x.VariableName, x => x.InputTagId)
									},
									MathInfo = new TagInfo.TagMathInfo
									{
										IsScaling = tag.IsScaling,
										MaxEu = tag.MaxEu,
										MaxRaw = tag.MaxRaw,
										MinEu = tag.MinEu,
										MinRaw = tag.MinRaw,
									},
									SourceInfo = tag.SourceId == (int)CustomSource.System
										? new TagInfo.TagSourceInfo
										{
											Id = tag.SourceId,
											Name = CustomSource.System.ToString(),
											Item = null,
											Type = SourceType.Custom,
										}
										: tag.SourceId == (int)CustomSource.Manual 
										? new TagInfo.TagSourceInfo
										{
											Id = tag.SourceId,
											Name = CustomSource.Manual.ToString(),
											Item = null,
											Type = SourceType.Custom,
										}
										: tag.SourceId == (int)CustomSource.Calculated
										? new TagInfo.TagSourceInfo
										{
											Id = tag.SourceId,
											Name = CustomSource.Calculated.ToString(),
											Item = null,
											Type = SourceType.Custom,
										} :
										source != null 
										? new TagInfo.TagSourceInfo
										{
											Id = tag.SourceId,
											Name = source.Name,
											Item = tag.SourceItem,
											Type = source.Type,
										}
										: new TagInfo.TagSourceInfo
										{
											Id = tag.SourceId,
											Name = "Unknown",
											Item = null,
											Type = SourceType.Unknown,
										},
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
