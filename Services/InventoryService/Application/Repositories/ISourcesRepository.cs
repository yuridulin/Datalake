using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Application.Repositories;

public interface ISourcesRepository : IRepository<SourceEntity, int>
{
}
