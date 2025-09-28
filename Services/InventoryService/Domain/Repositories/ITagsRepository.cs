using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Domain.Repositories;

public interface ITagsRepository : IRepository<TagEntity, int>
{
}
