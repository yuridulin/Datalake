using Datalake.Domain.Entities;

namespace Datalake.Data.Application.Interfaces.Repositories;

public interface ITagsValuesRepository
{
	Task<bool> WriteAsync(IEnumerable<TagValue> batch);

	Task<IEnumerable<TagValue>> GetRangeAsync(IEnumerable<int> tagsIdentifiers, DateTime from, DateTime to);

	Task<IEnumerable<TagValue>> GetExactAsync(IEnumerable<int> tagsIdentifiers, DateTime exact);

	Task<IEnumerable<TagValue>> GetLastAsync(IEnumerable<int> tagsIdentifiers);

	Task<IEnumerable<TagValue>> GetAllLastAsync();
}
