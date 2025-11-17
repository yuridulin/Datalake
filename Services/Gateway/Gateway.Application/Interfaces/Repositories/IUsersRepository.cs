using Datalake.Domain.Entities;

namespace Datalake.Gateway.Application.Interfaces.Repositories;

public interface IUsersRepository
{
	Task<User?> GetByGuidAsync(Guid guid, CancellationToken ct = default);

	Task<User?> GetByLoginAsync(string login, CancellationToken ct = default);
}
