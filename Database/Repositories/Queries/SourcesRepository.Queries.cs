using Datalake.Database.Enums;
using Datalake.Database.Models.Sources;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class SourcesRepository
{
	static int[] CustomSourcesId = Enum.GetValues<CustomSource>().Cast<int>().ToArray();

	/// <summary>
	/// Запрос информации о источниках без связей
	/// </summary>
	/// <param name="withCustom">Включать ли системные источники в запрос</param>
	public IQueryable<SourceInfo> GetInfo(bool withCustom = false)
	{
		var query = 
			from source in db.Sources
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

	/// <summary>
	/// Запрос информации о источниках вместе со списками зависящих тегов
	/// </summary>
	public IQueryable<SourceWithTagsInfo> GetInfoWithTags()
	{
		var query =
			from source in db.Sources
			select new SourceWithTagsInfo
			{
				Id = source.Id,
				Address = source.Address,
				Name = source.Name,
				Type = source.Type,
				Tags =
					from tag in db.Tags
					where tag.SourceId == source.Id
					select new SourceTagInfo
					{
						Guid = tag.GlobalGuid,
						Item = tag.SourceItem ?? string.Empty,
						Name = tag.Name,
						Type = tag.Type,
						Interval = tag.Interval,
					}
			};

		return query;
	}

	/// <summary>
	/// Запрос информации о зависящих от источника тегов по его идентификатору
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	public IQueryable<SourceTagInfo> GetExistTags(int id)
	{
		var query = db.Tags
			.Where(x => x.SourceId == id)
			.Select(x => new SourceTagInfo
			{
				Guid = x.GlobalGuid,
				Name = x.Name,
				Type = x.Type,
				Item = x.SourceItem ?? string.Empty,
				Interval = x.Interval,
			});

		return query;
	}
}
