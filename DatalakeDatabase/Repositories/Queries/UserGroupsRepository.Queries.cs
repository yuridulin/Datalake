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

	public IQueryable<UserGroupDetailedInfo> GetWithChildsAndUsers()
	{
		var query = from g in db.UserGroups
								from rel in db.UserGroupRelations.LeftJoin(x => x.UserGroupGuid == g.Guid)
								from u in db.Users.LeftJoin(x => x.Guid == rel.UserGuid)
								from c in db.UserGroups.LeftJoin(x => x.ParentGuid == g.Guid)
								group new { g, rel, u, c } by g into groupping
								select new UserGroupDetailedInfo
								{
									Guid = groupping.Key.Guid,
									Name = groupping.Key.Name,
									Description = groupping.Key.Description,
									ParentGroupGuid = groupping.Key.ParentGuid,
									Subgroups = groupping
										.Select(x => new UserGroupInfo
										{
											Guid = x.c.Guid,
											Name = x.c.Name,
											Description = x.c.Description,
										})
										.ToArray(),
									Users = groupping
										.Select(x => new UserGroupUsersInfo
										{
											Guid = x.u.Guid,
											FullName = x.u.FullName,
											AccessType = x.u.AccessType,
										})
										.ToArray(),
								};

		return query;
	}
}
