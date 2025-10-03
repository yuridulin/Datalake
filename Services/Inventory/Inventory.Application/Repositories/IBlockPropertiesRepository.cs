using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface IBlockPropertiesRepository : IRepository<BlockPropertyEntity, int>
{
}
