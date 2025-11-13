using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Models.AccessRules;
using Datalake.Contracts.Public.Models.Blocks;
using Datalake.Contracts.Public.Models.Sources;
using Datalake.Contracts.Public.Models.Tags;
using Datalake.Contracts.Public.Models.UserGroups;
using Datalake.Contracts.Public.Models.Users;
using Datalake.Inventory.Application.Queries;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class AccessRulesQueriesService(InventoryDbContext context) : IAccessRulesQueriesService
{
	public async Task<IEnumerable<AccessRightsInfo>> GetAsync(
		Guid? userGuid = null,
		Guid? userGroupGuid = null,
		int? sourceId = null,
		int? blockId = null,
		int? tagId = null,
		CancellationToken ct = default)
	{
		return await context.AccessRules
			.Include(rule => rule.Block)
			.Include(rule => rule.Tag).ThenInclude(x => x!.Source)
			.Include(rule => rule.Source)
			.Include(rule => rule.User).ThenInclude(x => x!.EnergoId)
			.Include(rule => rule.UserGroup)
			.Where(rule => !rule.IsGlobal)
			.Where(rule => !userGuid.HasValue || rule.UserGuid == userGuid)
			.Where(rule => !userGroupGuid.HasValue || rule.UserGroupGuid == userGroupGuid)
			.Where(rule => !sourceId.HasValue || rule.SourceId == sourceId)
			.Where(rule => !blockId.HasValue || rule.BlockId == blockId)
			.Where(rule => !tagId.HasValue || rule.TagId == tagId)
			.AsNoTracking()
			.Select(rule => new AccessRightsInfo
			{
				Id = rule.Id,
				AccessType = rule.AccessType,
				IsGlobal = rule.IsGlobal,
				User = rule.User == null || rule.User.IsDeleted ? null : new UserSimpleInfo
				{
					Guid = rule.User.Guid,
					FullName = rule.User.EnergoId == null
						? (rule.User.FullName ?? string.Empty)
						: $"{rule.User.EnergoId.LastName} {rule.User.EnergoId.FirstName} {rule.User.EnergoId.MiddleName}",
				},
				UserGroup = rule.UserGroup == null || rule.UserGroup.IsDeleted ? null : new UserGroupSimpleInfo
				{
					Guid = rule.UserGroup.Guid,
					Name = rule.UserGroup.Name,
				},
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
					SourceType = rule.Tag.Source == null ? SourceType.Unset : rule.Tag.Source.Type,
				}
			})
			.ToArrayAsync(ct);
	}
}
