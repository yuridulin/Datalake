using Datalake.Contracts.Models.LogModels;
using Datalake.Domain.Enums;
using Datalake.Domain.Extensions;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Infrastructure.Database.Extensions;
using LinqToDB;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class AuditQueriesService(InventoryDbLinqContext context) : IAuditQueriesService
{
	public async Task<IEnumerable<LogInfo>> GetAsync(
		int? lastId = null,
		int? firstId = null,
		int? take = null,
		int? sourceId = null,
		int? blockId = null,
		int? tagId = null,
		Guid? userGuid = null,
		Guid? groupGuid = null,
		LogCategory[]? categories = null,
		LogType[]? types = null,
		Guid? authorGuid = null,
		CancellationToken ct = default)
	{
		var query =
			from log in context.Audit
			from author in context.Users.AsSimpleInfo(context.EnergoId).LeftJoin(x => x.Guid == log.AuthorGuid)
			from source in context.Sources.AsSimpleInfo().LeftJoin(x => x.Id == log.AffectedSourceId)
			from tag in context.Tags.AsSimpleInfo(context.Sources).LeftJoin(x => x.Id == log.AffectedTagId)
			from block in context.Blocks.AsSimpleInfo().LeftJoin(x => x.Id == log.AffectedBlockId)
			from user in context.Users.AsSimpleInfo(context.EnergoId).LeftJoin(x => x.Guid == log.AffectedUserGuid)
			from usergroup in context.UserGroups.AsSimpleInfo().LeftJoin(x => x.Guid == log.AffectedUserGroupGuid)
			where
				(authorGuid == null || authorGuid.Value == log.AuthorGuid) &&
				(sourceId == null || sourceId.Value == log.AffectedSourceId) &&
				(tagId == null || tagId.Value == log.AffectedTagId) &&
				(blockId == null || blockId.Value == log.AffectedBlockId) &&
				(userGuid == null || userGuid.Value == log.AffectedUserGuid) &&
				(groupGuid == null || groupGuid.Value == log.AffectedUserGroupGuid) &&
				(types == null || types.Contains(log.Type)) &&
				(categories == null || categories.Contains(log.Category))
			select new LogInfo
			{
				Id = log.Id,
				Category = log.Category,
				DateString = log.Date.HierarchicalWithMilliseconds(),
				Text = log.Text,
				Type = log.Type,
				Details = log.Details,
				Author = author,
				AffectedSource = source,
				AffectedBlock = block,
				AffectedTag = tag,
				AffectedUser = user,
				AffectedUserGroup = usergroup,
			};

		query = query
			.OrderByDescending(x => x.Id);

		if (lastId.HasValue)
			query = query.Where(x => x.Id > lastId.Value);
		else if (firstId.HasValue)
			query = query.Where(x => x.Id < firstId.Value);

		if (take.HasValue)
			query = query.Take(take.Value);

		return await query.ToArrayAsync(ct);
	}
}
