using DatalakeDatabase.ApiModels.Sources;
using LinqToDB;

namespace DatalakeDatabase.Repositories;

public partial class SourcesRepository
{
	public IQueryable<SourceInfo> GetSources()
	{
		var query = from source in db.Sources
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
