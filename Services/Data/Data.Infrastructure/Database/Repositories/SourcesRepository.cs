using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Data.Infrastructure.Database.Repositories;

[Scoped]
public class SourcesRepository(DataDbContext context) : ISourcesRepository
{
	public async Task<Source?> GetByIdAsync(int sourceId, CancellationToken cancellationToken)
	{
		return await context.Sources
			.Where(x => x.Id == sourceId)
			.AsNoTracking()
			.FirstOrDefaultAsync(cancellationToken);
	}
}
