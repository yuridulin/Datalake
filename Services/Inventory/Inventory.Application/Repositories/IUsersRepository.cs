using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces.Persistent;

namespace Datalake.Inventory.Application.Repositories;

public interface IUsersRepository : IRepository<User, Guid>
{
	Task<User?> GetByLoginAsync(string login);
}
