using Datalake.Domain.ValueObjects;
using Datalake.Gateway.Application.Interfaces;

namespace Datalake.Gateway.Infrastructure.Database.Services;

public class UserAccessService : IUserAccessService
{
	public Task<UserAccessValue> AuthenticateAsync(Guid userGuid, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}
}
