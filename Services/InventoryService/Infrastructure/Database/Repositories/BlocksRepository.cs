using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PrivateApi.Attributes;

namespace Datalake.InventoryService.Infrastructure.Database.Repositories;

[Scoped]
public class BlocksRepository(InventoryEfContext context) : EfRepository<BlockEntity, int>(context), IBlocksRepository
{
}
