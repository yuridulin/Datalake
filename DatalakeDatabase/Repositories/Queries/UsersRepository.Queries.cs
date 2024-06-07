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
				Guid = x.Guid,
				Login = x.Login,
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
				Guid = x.Guid,
				Login = x.Login,
				FullName = x.FullName,
				AccessType = x.AccessType,
				IsStatic = !string.IsNullOrEmpty(x.StaticHost),
				Hash = !string.IsNullOrEmpty(x.StaticHost) ? x.PasswordHash : string.Empty,
				StaticHost = x.StaticHost,
			});
	}
}
