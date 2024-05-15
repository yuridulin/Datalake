using DatalakeDatabase.ApiModels.Users;

namespace DatalakeDatabase.Repositories
{
	public partial class UsersRepository
	{
		public IQueryable<UserInfo> GetInfo()
		{
			return db.Users
				.Select(x => new UserInfo
				{
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
					AccessType = x.AccessType,
					Hash = x.Hash,
					IsStatic = !string.IsNullOrEmpty(x.StaticHost),
					StaticHost = x.StaticHost,
					LoginName = x.Name,
					FullName = x.FullName,
				});
		}
	}
}
