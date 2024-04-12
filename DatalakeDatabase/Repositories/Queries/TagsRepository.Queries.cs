using DatalakeDatabase.ApiModels.Tags;
using DatalakeDatabase.Enums;
using LinqToDB;

namespace DatalakeDatabase.Repositories;

public partial class TagsRepository
{
	public IQueryable<TagInfo> GetTagsWithSources()
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
										} :source != null 
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
}
