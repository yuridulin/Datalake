using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.AccessRights;
using Datalake.ApiClasses.Models.Blocks;
using Datalake.ApiClasses.Models.Sources;
using Datalake.ApiClasses.Models.Tags;
using Datalake.ApiClasses.Models.UserGroups;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class UserGroupsRepository
{
	public async Task<UserGroupTreeInfo[]> GetTreeAsync()
	{
		var groups = await db.UserGroups
			.Select(x => new UserGroupTreeInfo
			{
				Guid = x.Guid,
				Name = x.Name,
				ParentGuid = x.ParentGuid,
				Description = x.Description,
			})
			.ToArrayAsync();

		return ReadChildren(null);

		UserGroupTreeInfo[] ReadChildren(Guid? guid)
		{
			var selected = groups
				.Where(x => x.ParentGuid == guid)
				.ToArray();

			foreach (var item in selected)
			{
				item.Children = ReadChildren(item.Guid);
			};

			return selected;
		}
	}

	public async Task<UserGroupInfo[]> GetWithParentsAsync(Guid groupGuid)
	{
		var groups = await db.UserGroups
			.Select(x => new UserGroupTreeInfo
			{
				Guid = x.Guid,
				Name = x.Name,
				Description = x.Description,
				ParentGuid = x.ParentGuid,
			})
			.ToArrayAsync();

		var parents = new List<UserGroupInfo>();
		Guid? seekGuid = groupGuid;

		do
		{
			var group = groups
				.Where(x => x.Guid == groupGuid)
				.FirstOrDefault();

			if (group == null)
				break;

			parents.Add(new UserGroupInfo { Name = group.Name, Guid = group.Guid });
			seekGuid = group.ParentGuid;
		}
		while (seekGuid != null);

		return groups;
	}

	public IQueryable<UserGroupInfo> GetInfo()
	{
		return db.UserGroups
			.Select(x => new UserGroupInfo
			{
				Guid = x.Guid,
				Name = x.Name,
				Description = x.Description,
				ParentGroupGuid = x.ParentGuid,
			});
	}

	public async Task<UserGroupDetailedInfo> GetWithDetails(Guid? guid)
	{
		var group = await db.UserGroups
			.Select(x => new UserGroupDetailedInfo
			{
				Guid = x.Guid,
				Name = x.Name,
				Description = x.Description,
				ParentGroupGuid = x.ParentGuid,
				Users = Array.Empty<UserGroupUsersInfo>(),
				AccessRights = Array.Empty<AccessRightsForOneInfo>(),
				Subgroups = Array.Empty<UserGroupInfo>(),
			})
			.FirstOrDefaultAsync(x => x.Guid == guid)
			?? throw new NotFoundException(message: "группа пользователей " + guid);

		group.Subgroups = await db.UserGroups
			.Where(x => x.ParentGuid == guid)
			.Select(x => new UserGroupInfo
			{
				Guid = x.Guid,
				Name = x.Name,
			})
			.ToArrayAsync();

		group.Users = await (
			from rel in db.UserGroupRelations.Where(x => x.UserGroupGuid == guid)
			from u in db.Users.InnerJoin(x => x.Guid == rel.UserGuid)
			select new UserGroupUsersInfo
			{
				Guid = u.Guid,
				FullName = u.FullName,
				AccessType = rel.AccessType,
			}
		).ToArrayAsync();

		var accessRights = await (
			from access in db.AccessRights.Where(x => x.UserGroupGuid == guid)
			from source in db.Sources.LeftJoin(x => x.Id == access.SourceId)
			from block in db.Blocks.LeftJoin(x => x.Id == access.BlockId)
			from tag in db.Tags.LeftJoin(x => x.Id == access.TagId)
			select new AccessRightsForOneInfo
			{
				Id = access.Id,
				AccessType = access.AccessType,
				IsGlobal = access.IsGlobal,
				Source = source == null ? null : new SourceSimpleInfo
				{
					Id = source.Id,
					Name = source.Name,
				},
				Block = block == null ? null : new BlockSimpleInfo
				{
					Id = block.Id,
					Guid = block.GlobalId,
					Name = block.Name,
				},
				Tag = tag == null ? null : new TagSimpleInfo
				{
					Id = tag.Id,
					Guid = tag.GlobalGuid,
					Name = tag.Name,
				},
			}
		).ToArrayAsync();

		group.AccessRights = accessRights.Where(x => !x.IsGlobal).ToArray();
		group.GlobalAccessType = accessRights.FirstOrDefault(x => x.IsGlobal)?.AccessType ?? AccessType.NotSet;

		return group;
	}
}
