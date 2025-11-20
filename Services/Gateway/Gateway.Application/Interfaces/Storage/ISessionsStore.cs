using Datalake.Domain.Entities;

namespace Datalake.Gateway.Application.Interfaces.Storage;

public interface ISessionsStore
{
	Task<UserSession?> GetAsync(string token);

	Task SetAsync(string token, UserSession session);

	Task RefreshAsync(UserSession session);

	Task RemoveAsync(string token);
}
