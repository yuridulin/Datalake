using DatalakeApiClasses.Models.Abstractions;
using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Models.Users;
using DatalakeDatabase.Extensions;
using LinqToDB;

namespace DatalakeDatabase.Repositories
{
	public partial class UsersRepository
	{
		public IQueryable<UserInfo> GetInfo()
		{
			return db.Users
				.Select(x => new UserInfo
				{
					UserGuid = x.UserGuid.ToString(),
					LoginName = x.Name,
					FullName = x.FullName,
					AccessType = x.AccessType,
					IsStatic = !string.IsNullOrEmpty(x.StaticHost),
				});
		}

		public IQueryable<UserDetailInfo> GetDetailInfo()
		{
			return db.Users
				.Select(x => new UserDetailInfo
				{
					UserGuid = x.UserGuid.ToString(),
					AccessType = x.AccessType,
					Hash = !string.IsNullOrEmpty(x.StaticHost) ? x.Hash : string.Empty,
					IsStatic = !string.IsNullOrEmpty(x.StaticHost),
					StaticHost = x.StaticHost,
					LoginName = x.Name,
					FullName = x.FullName,
				});
		}

		static readonly UserGroupAccess[] UserHasAccessRelations = [UserGroupAccess.Member, UserGroupAccess.Administrator];

		public async Task<UserGroupTreeInfo[]> GetUserGroupsTreeAsync(Guid userGuid)
		{
			var user = await db.Users.FirstOrDefaultAsync(x => x.UserGuid ==  userGuid);
			var groupsQuery = from userGroup in db.UserGroups
												from rel in db.UserGroupRelations
												 .Where(x => x.UserGuid == userGuid)
												 .LeftJoin(x => x.UserGroupGuid == userGroup.UserGroupGuid)
												group new { userGroup, rel } by userGroup into g
												select new
												{
													Id = g.Key.UserGroupGuid.ToString(),
													ParentId = g.Key.ParentGroupGuid.ToString(),
													g.Key.Name,
													Relations = g.Select(x => x.rel != null ? x.rel.AccessType : UserGroupAccess.Not).ToArray(),
												};

			var groups = groupsQuery.ToArray();

			var userGroups = groups.Where(x => x.Relations.Intersect(UserHasAccessRelations).Any()).ToArray();

			return userGroups
				.Select(x => new UserGroupTreeInfo
				{
					Guid = x.Id,
					Name = x.Name,
					Children = ReadChildren(x.Id),
				})
				.ToArray();

			UserGroupTreeInfo[] ReadChildren(string? id)
			{
				return groups
					.Where(x => x.ParentId == id)
					.Select(x => new UserGroupTreeInfo
					{
						Name = x.Name,
						Guid = x.Id,
						Children = ReadChildren(id)
					})
					.ToArray();
			}
		}

		public async Task<List<UserGroupInfo>> GetUserGroupsAsync(Guid userGuid)
		{
			var groupsTree = await GetUserGroupsTreeAsync(userGuid);
			var groups = new List<UserGroupInfo>();

			foreach (var group in groupsTree)
			{
				ExtractGroups(group);
			}

			void ExtractGroups(UserGroupTreeInfo userGroupInfo)
			{
				groups.Add(userGroupInfo);
				foreach (var child in userGroupInfo.Children) ExtractGroups(child);
			}

			return groups;
		}

		public async Task<IRights> GetRightsAsync(Guid userGuid)
		{
			var groups = await GetUserGroupsAsync(userGuid);

			var rightsArray = await db.AccessRights
				.Where(x => groups.Select(g => g.Guid).Contains(x.UserGroupGuid.ToString()) || x.UserGuid == userGuid)
				.ToArrayAsync();

			return rightsArray.Merge();
		}
	}
}
