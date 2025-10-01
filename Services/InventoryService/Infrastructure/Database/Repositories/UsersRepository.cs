using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PrivateApi.Attributes;

namespace Datalake.InventoryService.Infrastructure.Database.Repositories;

[Scoped]
public class UsersRepository(InventoryEfContext context) : EfRepository<UserEntity, Guid>(context), IUsersRepository
{
}
