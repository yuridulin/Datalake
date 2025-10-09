using Datalake.Domain.Entities;

namespace Datalake.Gateway.Application.Interfaces;

public interface ISessionsCache
{
	Task<UserSession?> GetAsync(string token);

	Task SetAsync(string token, UserSession session);

	Task RefreshAsync(UserSession session);

	Task RemoveAsync(string token);
}
