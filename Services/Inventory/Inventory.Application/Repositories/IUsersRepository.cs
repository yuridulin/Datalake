using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces;

namespace Datalake.Inventory.Application.Repositories;

public interface IUsersRepository : IRepository<User, Guid>
{
	Task<User?> GetByLoginAsync(string login, CancellationToken ct = default);
}
