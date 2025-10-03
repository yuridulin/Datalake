using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface IUsersRepository : IRepository<UserEntity, Guid>
{
}
