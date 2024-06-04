using DatalakeApiClasses.Models.UserGroups;
using LinqToDB;

namespace DatalakeDatabase.Repositories;

public partial class UserGroupsRepository
{
	public async Task<UserGroupTreeInfo[]> GetTreeAsync()
	{
		var groups = await db.UserGroups
			.Select(x => new UserGroupTreeInfo
			{
				UserGroupGuid = x.UserGroupGuid,
				Name = x.Name,
				ParentGuid = x.ParentGroupGuid,
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
				item.Children = ReadChildren(guid);
			};

			return selected;
		}
	}

	public async Task<UserGroupInfo[]> GetWithParentsAsync(Guid groupGuid)
	{
		var groups = await db.UserGroups
			.Select(x => new UserGroupTreeInfo
			{
				UserGroupGuid = x.UserGroupGuid,
				Name = x.Name,
				Description = x.Description,
				ParentGuid = x.ParentGroupGuid,
			})
			.ToArrayAsync();

		var parents = new List<UserGroupInfo>();
		Guid? seekGuid = groupGuid;

		do
		{
			var group = groups
				.Where(x => x.UserGroupGuid == groupGuid)
				.FirstOrDefault();

			if (group == null)
				break;

			parents.Add(new UserGroupInfo { Name = group.Name, UserGroupGuid = group.UserGroupGuid });
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
				UserGroupGuid = x.UserGroupGuid,
				Name = x.Name,
				Description = x.Description,
			});
	}

	public IQueryable<UserGroupDetailedInfo> GetWithChildsAndUsers()
	{
		var query = from g in db.UserGroups
								from rel in db.UserGroupRelations.LeftJoin(x => x.UserGroupGuid == g.UserGroupGuid)
								from u in db.Users.LeftJoin(x => x.UserGuid == rel.UserGuid)
								from c in db.UserGroups.LeftJoin(x => x.ParentGroupGuid == g.UserGroupGuid)
								group new { g, rel, u, c } by g into groupping
								select new UserGroupDetailedInfo
								{
									UserGroupGuid = groupping.Key.UserGroupGuid,
									Name = groupping.Key.Name,
									Description = groupping.Key.Description,
									Subgroups = groupping
										.Select(x => new UserGroupInfo
										{
											UserGroupGuid = x.c.UserGroupGuid,
											Name = x.c.Name,
											Description = x.c.Description,
										})
										.ToArray(),
									Users = groupping
										.Select(x => new UserGroupUsersInfo
										{
											UserGuid = x.u.UserGuid,
											AccessType = x.u.AccessType,
										})
										.ToArray(),
								};

		return query;
	}
}
