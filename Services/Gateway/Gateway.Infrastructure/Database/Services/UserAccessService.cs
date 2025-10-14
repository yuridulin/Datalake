using Datalake.Gateway.Application.Interfaces;
using Datalake.Shared.Application.Entities;

namespace Datalake.Gateway.Infrastructure.Database.Services;

public class UserAccessService : IUserAccessService
{
	public Task<UserAccessValue> AuthenticateAsync(Guid userGuid, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}
}
