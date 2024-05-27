using DatalakeApiClasses.Models.Users;
using LinqToDB;

namespace DatalakeDatabase.Repositories;

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
}
