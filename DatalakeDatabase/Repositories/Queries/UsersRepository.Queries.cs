using DatalakeDatabase.ApiModels.Users;

namespace DatalakeDatabase.Repositories
{
	public partial class UsersRepository
	{
		public IQueryable<UserInfo> GetUsers()
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
	}
}
