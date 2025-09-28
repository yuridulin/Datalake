using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PrivateApi.Attributes;

namespace Datalake.InventoryService.Infrastructure.Database.Repositories;

[Scoped]
public class TagRepository(InventoryEfContext context) : EfRepository<TagEntity, int>(context), ITagsRepository
{
}
