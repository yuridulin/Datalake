using Datalake.InventoryService.Application.Interfaces.Persistent;
using Datalake.InventoryService.Domain.Entities;

namespace Datalake.InventoryService.Application.Repositories;

public interface IAuditRepository : IRepository<AuditEntity, int>
{
}
