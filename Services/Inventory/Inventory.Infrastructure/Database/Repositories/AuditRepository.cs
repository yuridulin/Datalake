using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Domain.Entities;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Shared.Application.Attributes;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class AuditRepository(InventoryEfContext context) : EfRepository<AuditEntity, int>(context), IAuditRepository
{
}
