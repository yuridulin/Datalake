using Datalake.Shared.Application.Entities;

namespace Datalake.Gateway.Application.Interfaces;

public interface IUserAccessService
{
	Task<UserAccessValue> AuthenticateAsync(Guid userGuid, CancellationToken ct = default);
}