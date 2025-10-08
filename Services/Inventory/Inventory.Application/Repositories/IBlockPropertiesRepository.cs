using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces.Persistent;

namespace Datalake.Inventory.Application.Repositories;

public interface IBlockPropertiesRepository : IRepository<BlockProperty, int>
{
}
