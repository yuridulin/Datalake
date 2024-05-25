using DatalakeDatabase.ApiModels.Sources;
using DatalakeDatabase.Enums;
using LinqToDB;

namespace DatalakeDatabase.Repositories;

public partial class SourcesRepository
{
	static int[] CustomSourcesId = Enum.GetValues<CustomSource>().Cast<int>().ToArray();

	public IQueryable<SourceInfo> GetInfo(bool withCustom = false)
	{
		var query = from source in db.Sources
								where withCustom || !CustomSourcesId.Contains(source.Id)
								select new SourceInfo
								{
									Id = source.Id,
									Name = source.Name,
									Address = source.Address,
									Description = source.Description,
									Type = source.Type,
								};

		return query;
	}

	public IQueryable<SourceWithTagsInfo> GetInfoWithTags()
	{
		var query = from s in db.Sources
								from t in db.Tags.LeftJoin(x => x.SourceId == s.Id)
								group new { s, t } by s into g
								select new SourceWithTagsInfo
								{
									Id = g.Key.Id,
									Address = g.Key.Address,
									Name = g.Key.Name,
									Type = g.Key.Type,
									Tags = g
										.Select(x => new SourceTagInfo
										{
											Id = x.t.Id,
											Item = x.t.SourceItem ?? string.Empty,
											Name = x.t.Name,
											Type = x.t.Type,
										})
										.ToArray(),
								};

		return query;
	}

	public IQueryable<SourceTagInfo> GetExistTags(int id)
	{
		var query = db.Tags
			.Where(x => x.SourceId == id)
			.Select(x => new SourceTagInfo
			{
				Id = x.Id,
				Name = x.Name,
				Type = x.Type,
				Item = x.SourceItem ?? string.Empty,
			});

		return query;
	}
}
