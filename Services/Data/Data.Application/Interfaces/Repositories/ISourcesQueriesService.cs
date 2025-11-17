using Datalake.Data.Application.Models.Sources;

namespace Datalake.Data.Application.Interfaces.Repositories;

public interface ISourcesQueriesService
{
	Task<SourceSettingsDto?> GetByIdAsync(int sourceId, CancellationToken cancellationToken);

	Task<IEnumerable<SourceSettingsDto>> GetAllAsync(CancellationToken cancellationToken);
}
