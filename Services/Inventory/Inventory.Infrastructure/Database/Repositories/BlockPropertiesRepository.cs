using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Shared.Application.Attributes;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class BlockPropertiesRepository(InventoryDbContext context) : DbRepository<BlockProperty, int>(context), IBlockPropertiesRepository
{
}
