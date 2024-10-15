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

	public IQueryable<UserGroupDetailedInfo> GetWithDetails()
	{
		var query =
			from usergroup in db.UserGroups
			select new UserGroupDetailedInfo
			{
				Guid = usergroup.Guid,
				Name = usergroup.Name,
				Description = usergroup.Description,
				ParentGroupGuid = usergroup.ParentGuid,
				Users =
					from rel in db.UserGroupRelations.LeftJoin(x => x.UserGroupGuid == usergroup.Guid)
					from u in db.Users.InnerJoin(x => x.Guid == rel.UserGuid)
					select new UserGroupUsersInfo
					{
						Guid = u.Guid,
						FullName = u.FullName,
						AccessType = rel.AccessType,
					},
				AccessRights =
					from rights in db.AccessRights.InnerJoin(x => x.UserGroupGuid == usergroup.Guid)
					from source in db.Sources.LeftJoin(x => x.Id == rights.SourceId)
					from block in db.Blocks.LeftJoin(x => x.Id == rights.BlockId)
					from tag in db.Tags.LeftJoin(x => x.Id == rights.TagId)
					select new AccessRightsForOneInfo
					{
						Id = rights.Id,
						IsGlobal = rights.IsGlobal,
						AccessType = rights.AccessType,
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
					},
				Subgroups =
					from subgroup in db.UserGroups.LeftJoin(x => x.ParentGuid == usergroup.Guid)
					select new UserGroupSimpleInfo
					{
						Guid = subgroup.Guid,
						Name = subgroup.Name,
					},
			};

		return query;
	}
}
