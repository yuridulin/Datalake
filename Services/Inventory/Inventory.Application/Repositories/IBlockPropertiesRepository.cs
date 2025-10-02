using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface IBlockPropertiesRepository : IRepository<BlockPropertyEntity, int>
{
}
