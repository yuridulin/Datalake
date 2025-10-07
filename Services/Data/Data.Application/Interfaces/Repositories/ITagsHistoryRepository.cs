using Datalake.Domain.ValueObjects;

namespace Datalake.Data.Application.Interfaces.Repositories;

public interface ITagsHistoryRepository
{
	Task WriteAsync(IEnumerable<TagHistory> batch);

	Task<IEnumerable<TagHistory>> GetRangeAsync(IEnumerable<int> tagsIdentifiers, DateTime from, DateTime to);

	Task<IEnumerable<TagHistory>> GetExactAsync(IEnumerable<int> tagsIdentifiers, DateTime exact);

	Task<IEnumerable<TagHistory>> GetLastAsync(IEnumerable<int> tagsIdentifiers);

	Task<IEnumerable<TagHistory>> GetAllLastAsync();
}
