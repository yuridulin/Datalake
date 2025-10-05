using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class EnergoIdRepository(InventoryDbContext context) : IEnergoIdRepository
{
	public async Task<IEnumerable<EnergoId>> GetAsync(CancellationToken ct = default)
	{
		return await context.EnergoIdView.ToArrayAsync(ct);
	}
}
