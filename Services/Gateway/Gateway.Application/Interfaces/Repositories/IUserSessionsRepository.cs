using Datalake.Domain.Entities;

namespace Datalake.Gateway.Application.Interfaces.Repositories;

public interface IUserSessionsRepository
{
	Task<UserSession?> GetByTokenAsync(string token, CancellationToken ct = default);

	Task<UserSession?> GetByGuidAsync(Guid guid, CancellationToken ct = default);

	Task<IEnumerable<UserSession>> GetAllAsync(CancellationToken ct = default);

	Task AddAsync(UserSession userSession, CancellationToken ct = default);

	Task UpdateAsync(UserSession userSession, CancellationToken ct = default);

	Task DeleteAsync(UserSession userSession, CancellationToken ct = default);
}
