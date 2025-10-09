using Datalake.Gateway.Application.Interfaces;
using Datalake.Shared.Application.Entities;

namespace Datalake.Gateway.Infrastructure.Database.Services;

public class UserAccessService : IUserAccessService
{
	public Task<UserAccessEntity> AuthenticateAsync(Guid userGuid, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}
}
