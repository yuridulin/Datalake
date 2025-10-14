using Datalake.Domain.ValueObjects;

namespace Datalake.Gateway.Application.Interfaces;

public interface IUserAccessService
{
	Task<UserAccessValue> AuthenticateAsync(Guid userGuid, CancellationToken ct = default);
}