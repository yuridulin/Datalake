using Datalake.Contracts.Models.AccessRules;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Infrastructure.Database.Extensions;
using LinqToDB;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class AccessRulesQueriesService(InventoryDbLinqContext context) : IAccessRulesQueriesService
{
	public async Task<IEnumerable<AccessRightsInfo>> GetAsync(
		Guid? userGuid = null,
		Guid? userGroupGuid = null,
		int? sourceId = null,
		int? blockId = null,
		int? tagId = null,
		CancellationToken ct = default)
	{
		var query =
			from rule in context.AccessRules
			from block in context.Blocks.AsSimpleInfo().LeftJoin(x => x.Id == rule.BlockId)
			from tag in context.Tags.AsSimpleInfo(context.Sources).LeftJoin(x => x.Id == rule.TagId)
			from source in context.Sources.AsSimpleInfo().LeftJoin(x => x.Id == rule.SourceId)
			from user in context.Users.AsSimpleInfo(context.EnergoId).LeftJoin(x => x.Guid == rule.UserGuid)
			from usergroup in context.UserGroups.AsSimpleInfo().LeftJoin(x => x.Guid == rule.UserGroupGuid)
			where !rule.IsGlobal &&
				(blockId == null || blockId == rule.BlockId) &&
				(tagId == null || tagId == rule.TagId) &&
				(sourceId == null || sourceId == rule.SourceId) &&
				(userGuid == null || userGuid == rule.UserGuid) &&
				(userGroupGuid == null || userGroupGuid == rule.UserGroupGuid)
			select new AccessRightsInfo
			{
				Id = rule.Id,
				AccessType = rule.AccessType,
				IsGlobal = rule.IsGlobal,
				User = user,
				UserGroup = usergroup,
				Source = source,
				Block = block,
				Tag = tag,
			};

		return await query.ToArrayAsync(ct);
	}
}
