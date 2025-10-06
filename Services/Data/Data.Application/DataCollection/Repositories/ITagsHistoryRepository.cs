using Datalake.Domain.ValueObjects;

namespace Datalake.Data.Application.DataCollection.Repositories;

public interface ITagsHistoryRepository
{
	Task WriteAsync(IEnumerable<TagHistory> batch);
}
