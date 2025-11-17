using Datalake.Domain.Entities;

namespace Datalake.Data.Application.Interfaces.Repositories;

public interface ISourcesRepository
{
	Task<Source?> GetByIdAsync(int sourceId, CancellationToken cancellationToken);
}
