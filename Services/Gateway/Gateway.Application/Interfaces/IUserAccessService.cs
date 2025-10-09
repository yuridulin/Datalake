using Datalake.Shared.Application.Entities;

namespace Datalake.Gateway.Application.Interfaces;

public interface IUserAccessService
{
	Task<UserAccessEntity> AuthenticateAsync(Guid userGuid, CancellationToken ct = default);
}