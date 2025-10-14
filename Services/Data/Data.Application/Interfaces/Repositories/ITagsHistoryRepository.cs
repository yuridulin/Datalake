using Datalake.Domain.ValueObjects;

namespace Datalake.Data.Application.Interfaces.Repositories;

public interface ITagsHistoryRepository
{
	Task<bool> WriteAsync(IEnumerable<TagHistoryValue> batch);

	Task<IEnumerable<TagHistoryValue>> GetRangeAsync(IEnumerable<int> tagsIdentifiers, DateTime from, DateTime to);

	Task<IEnumerable<TagHistoryValue>> GetExactAsync(IEnumerable<int> tagsIdentifiers, DateTime exact);

	Task<IEnumerable<TagHistoryValue>> GetLastAsync(IEnumerable<int> tagsIdentifiers);

	Task<IEnumerable<TagHistoryValue>> GetAllLastAsync();
}
