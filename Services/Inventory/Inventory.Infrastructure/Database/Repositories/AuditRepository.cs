using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Shared.Application.Attributes;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class AuditRepository(InventoryDbContext context) : DbRepository<Log, int>(context), IAuditRepository
{
}
