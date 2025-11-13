using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Models.AccessRules;
using Datalake.Contracts.Public.Models.Blocks;
using Datalake.Contracts.Public.Models.Sources;
using Datalake.Contracts.Public.Models.Tags;
using Datalake.Contracts.Public.Models.UserGroups;
using Datalake.Inventory.Application.Queries;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class UsersGroupsQueriesService(InventoryDbContext context) : IUsersGroupsQueriesService
{
	private IQueryable<UserGroupInfo> QueryUserGroupInfo()
	{
		return context.UserGroups
			.Where(userGroup => !userGroup.IsDeleted)
			.AsNoTracking()
			.Select(userGroup => new { Group = userGroup, GlobalRule = userGroup.AccessRules.Where(r => r.IsGlobal).FirstOrDefault() })
			.Select(x => new UserGroupInfo
			{
				Guid = x.Group.Guid,
				Name = x.Group.Name,
				Description = x.Group.Description,
				ParentGroupGuid = x.Group.ParentGuid,
				GlobalAccessType = x.GlobalRule == null ? AccessType.None : x.GlobalRule.AccessType,
			});
	}

	public async Task<IEnumerable<UserGroupInfo>> GetAsync(
		CancellationToken ct = default)
	{
		return await QueryUserGroupInfo().ToArrayAsync(ct);
	}

	public async Task<UserGroupInfo?> GetAsync(
		Guid userGroupGuid,
		CancellationToken ct = default)
	{
		return await QueryUserGroupInfo().FirstOrDefaultAsync(x => x.Guid == userGroupGuid, ct);
	}

	public async Task<UserGroupDetailedInfo?> GetWithDetailsAsync(
		Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var userGroup = await GetAsync(userGroupGuid, ct);
		if (userGroup == null)
		{
			return null;
		}

		var groupUsers = await context.UserGroupRelations
			.Where(x => x.UserGroupGuid == userGroupGuid)
			.AsNoTracking()
			.Select(x => new UserGroupUsersInfo
			{
				Guid = x.User.Guid,
				FullName = x.User.FullName,
				AccessType = x.AccessType,
			})
			.ToArrayAsync(ct);

		var groupRules = await context.AccessRules
			.Where(rule => !rule.IsGlobal && rule.UserGroupGuid == userGroup.Guid)
			.AsNoTracking()
			.Select(rule => new AccessRightsForOneInfo
			{
				Id = rule.Id,
				IsGlobal = rule.IsGlobal,
				AccessType = rule.AccessType,
				Source = rule.Source == null || rule.Source.IsDeleted ? null : new SourceSimpleInfo
				{
					Id = rule.Source.Id,
					Name = rule.Source.Name,
				},
				Block = rule.Block == null || rule.Block.IsDeleted ? null : new BlockSimpleInfo
				{
					Id = rule.Block.Id,
					Guid = rule.Block.GlobalId,
					Name = rule.Block.Name,
				},
				Tag = rule.Tag == null || rule.Tag.IsDeleted ? null : new TagSimpleInfo
				{
					Id = rule.Tag.Id,
					Guid = rule.Tag.GlobalGuid,
					Name = rule.Tag.Name,
					Type = rule.Tag.Type,
					Resolution = rule.Tag.Resolution,
					SourceType = rule.Tag.Source == null || rule.Tag.Source.IsDeleted ? SourceType.Unset : rule.Tag.Source.Type,
				},
			})
			.Where(x => x.Tag != null || x.Block != null || x.Source != null)
			.ToArrayAsync(ct);

		var groupChildren = await context.UserGroups
			.Where(subGroup => subGroup.ParentGuid == userGroup.Guid)
			.AsNoTracking()
			.Select(subGroup => new UserGroupSimpleInfo
			{
				Guid = subGroup.Guid,
				Name = subGroup.Name,
			})
			.ToArrayAsync(ct);

		var result = new UserGroupDetailedInfo
		{
			Guid = userGroup.Guid,
			Name = userGroup.Name,
			Description = userGroup.Description,
			ParentGroupGuid = userGroup.ParentGroupGuid,
			GlobalAccessType = userGroup.GlobalAccessType,
			Users = groupUsers,
			AccessRights = groupRules,
			Subgroups = groupChildren,
		};

		return result;
	}
}
