using Datalake.Contracts.Models.Sources;

namespace Datalake.Data.Application.Interfaces.Storage;

public interface ISourcesActivityStore
{
	void Set(int sourceId, int tagsCount, bool isActive, int receivedCount);

	SourceActivityInfo Get(int sourceId);
}
