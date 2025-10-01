using Datalake.InventoryService.Infrastructure.Database;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Users;

namespace Datalake.InventoryService.Application.Features.Users;

public interface IUsersMemoryRepository
{
	Task<UserInfo> CreateAsync(InventoryEfContext db, UserAuthInfo user, UserCreateRequest userInfo);
	Task<bool> DeleteAsync(InventoryEfContext db, UserAuthInfo user, Guid userGuid);
	Task<bool> UpdateAsync(InventoryEfContext db, UserAuthInfo user, Guid userGuid, UserUpdateRequest request);

	UserInfo Get(UserAuthInfo user, Guid guid);
	UserInfo[] GetAll(UserAuthInfo user);
	UserEnergoIdInfo[] GetEnergoId(UserAuthInfo user);
	UserDetailInfo GetWithDetails(UserAuthInfo user, Guid guid);
}