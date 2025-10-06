using Datalake.Data.Application.DataCollection.Models;

namespace Datalake.Data.Application.DataCollection.Repositories;

public interface ISourcesSettingsRepository
{
	Task<IEnumerable<SourceSettingsDto>> GetAllAsync(CancellationToken cancellationToken);
}
