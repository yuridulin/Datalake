using Datalake.Domain.Entities;
using Datalake.Gateway.Application.Models;

namespace Datalake.Gateway.Application.Interfaces;

public interface ISessionsService
{
	Task<UserSessionInfo> GetAsync(string sessionToken, CancellationToken ct = default);

	Task<string> OpenAsync(User user, CancellationToken ct = default);

	Task CloseAsync(string token, CancellationToken ct = default);
}
