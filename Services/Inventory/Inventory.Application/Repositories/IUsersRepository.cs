using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface IUsersRepository : IRepository<UserEntity, Guid>
{
}
