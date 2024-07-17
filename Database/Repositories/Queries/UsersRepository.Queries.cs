using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.Users;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class UsersRepository
{
	public IQueryable<UserFlatInfo> GetFlatInfo()
	{
		return db.Users
			.Select(x => new UserFlatInfo
			{
				Guid = x.Guid,
				Login = x.Login,
				FullName = x.FullName,
				EnergoIdGuid = x.EnergoIdGuid,
				Type = x.Type,
			});
	}

	public IQueryable<UserInfo> GetInfo()
	{
		var query = from u in db.Users
								from rel in db.UserGroupRelations.LeftJoin(x => x.UserGuid == u.Guid)
								from g in db.UserGroups.LeftJoin(x => x.Guid == rel.UserGroupGuid)
								from urights in db.AccessRights.Where(x => x.IsGlobal).LeftJoin(x => x.UserGuid == u.Guid)
								from grights in db.AccessRights.Where(x => x.IsGlobal).LeftJoin(x => x.UserGroupGuid == g.Guid)
								group new { u, g, urights, grights } by u into g
								select new UserInfo
								{
									Login = g.Key.Login,
									Guid = g.Key.Guid,
									Type = g.Key.Type,
									FullName = g.Key.FullName,
									EnergoIdGuid = g.Key.EnergoIdGuid,
									UserGroups = g
										.Where(x => x.g != null)
										.Select(x => new UserGroupsInfo
										{
											Guid = x.g.Guid,
											Name = x.g.Name,
										})
										.ToArray(),
									AccessType = (AccessType)g
										.Select(x => Math.Max(
											(int)(x.urights != null ? x.urights.AccessType : AccessType.NotSet),
											(int)(x.urights != null ? x.urights.AccessType : AccessType.NotSet)
										))
										.DefaultIfEmpty((int)AccessType.NoAccess)
										.Max(),
								};

		return query;
	}

	public IQueryable<UserDetailInfo> GetDetailInfo()
	{
		var query = from u in db.Users
								from rel in db.UserGroupRelations.LeftJoin(x => x.UserGuid == u.Guid)
								from g in db.UserGroups.LeftJoin(x => x.Guid == rel.UserGroupGuid)
								from urights in db.AccessRights.Where(x => x.IsGlobal).LeftJoin(x => x.UserGuid == u.Guid)
								from grights in db.AccessRights.Where(x => x.IsGlobal).LeftJoin(x => x.UserGroupGuid == g.Guid)
								group new { u, g, urights, grights } by u into g
								select new UserDetailInfo
								{
									Login = g.Key.Login,
									Guid = g.Key.Guid,
									Type = g.Key.Type,
									FullName = g.Key.FullName,
									EnergoIdGuid = g.Key.EnergoIdGuid,
									UserGroups = g
										.Where(x => x.g != null)
										.Select(x => new UserGroupsInfo
										{
											Guid = x.g.Guid,
											Name = x.g.Name,
										})
										.ToArray(),
									AccessType = (AccessType)g
										.Select(x => Math.Max(
											(int)(x.urights != null ? x.urights.AccessType : AccessType.NotSet),
											(int)(x.urights != null ? x.urights.AccessType : AccessType.NotSet)
										))
										.DefaultIfEmpty((int)AccessType.NoAccess)
										.Max(),
									Hash = g.Key.PasswordHash,
									StaticHost = g.Key.StaticHost,
								};

		return query;
	}


	public async Task<string> GetEnergoIdApi()
	{
		return await db.Settings.Select(x => x.EnergoIdApi).FirstAsync();
	}
}
