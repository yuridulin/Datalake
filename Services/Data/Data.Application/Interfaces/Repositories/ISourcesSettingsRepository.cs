using Datalake.Data.Application.Models.Sources;

namespace Datalake.Data.Application.Interfaces.Repositories;

public interface ISourcesSettingsRepository
{
	Task<IEnumerable<SourceSettingsDto>> GetAllAsync(CancellationToken cancellationToken);
}
