using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Application.Repositories;

public interface IUsersRepository : IRepository<UserEntity, Guid>
{
}
