using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Users;
using LinqToDB;

namespace DatalakeDatabase.Repositories.Base;

public abstract class RepositoryBase
{
	protected static void CheckGlobalAccess(
		UserAuthInfo user,
		AccessType minimalAccess)
	{
		var hasAccess = user.Rights
			.Where(x => (int)minimalAccess <= (int)x.AccessType)
			.Where(x => x.IsGlobal)
			.Any();

		if (!hasAccess)
			throw NoAccess;
	}

	protected static void CheckAccessToSource(
		UserAuthInfo user,
		AccessType minimalAccess,
		int sourceId)
	{
		var hasAccess = user.Rights
			.Where(x => (int)minimalAccess <= (int)x.AccessType)
			.Where(x => x.IsGlobal || x.SourceId == sourceId)
			.Any();

		if (!hasAccess)
			throw NoAccess;
	}

	protected static async Task CheckAccessToBlockAsync(
		DatalakeContext db,
		UserAuthInfo user,
		AccessType minimalAccess,
		int blockId)
	{
		var blockWithParents = await new BlocksRepository(db).GetWithParentsAsync(blockId);
		var blocksId = blockWithParents.Select(x => x.Id).ToArray();

		var hasAccess = user.Rights
			.Where(x => (int)minimalAccess <= (int)x.AccessType)
			.Where(x => x.IsGlobal || blocksId.Contains(x.SourceId ?? -1))
			.Any();

		if (!hasAccess)
			throw NoAccess;
	}

	protected static async Task CheckAccessToTagAsync(
		DatalakeContext db,
		UserAuthInfo user,
		AccessType minimalAccess,
		int tagId)
	{
		var sourceQuery = from t in db.Tags.Where(x => x.Id == tagId)
											from s in db.Sources.InnerJoin(x => x.Id == t.SourceId)
											select s.Id;
		var source = await sourceQuery
			.DefaultIfEmpty(-1)
			.FirstOrDefaultAsync();

		var blocksQuery = from rel in db.BlockTags.Where(x => x.TagId == tagId)
											from b in db.Blocks.InnerJoin(x => x.Id == rel.BlockId)
											select b.Id;
		var blocksHasThisTag = await blocksQuery.ToArrayAsync();
		var repository = new BlocksRepository(db);
		var blocksHasThisTagWithParents = blocksHasThisTag
			.SelectMany(x =>
			{
				var blockWithParents = repository.GetWithParentsAsync(x).Result;
				return blockWithParents.Select(b => b.Id).ToArray();
			})
			.ToList();

		var hasAccess = user.Rights
			.Where(x => (int)minimalAccess <= (int)x.AccessType)
			.Where(x => x.IsGlobal || blocksHasThisTagWithParents.Contains(x.TagId ?? -1))
			.Any();

		if (!hasAccess)
			throw NoAccess;
	}

	protected static readonly ForbiddenException NoAccess = new(message: "нет доступа");
}
