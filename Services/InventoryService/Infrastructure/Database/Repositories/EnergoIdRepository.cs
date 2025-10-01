using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.PrivateApi.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Infrastructure.Database.Repositories;

[Scoped]
public class EnergoIdRepository(InventoryEfContext context) : IEnergoIdRepository
{
	public async Task<IEnumerable<EnergoIdEntity>> GetAsync(CancellationToken ct = default)
	{
		return await context.EnergoIdView.ToArrayAsync(ct);
	}
}
