using Datalake.Data.Host.Database.Entities;

namespace Datalake.Data.Host.Database.Interfaces;

public interface IWriteHistoryRepository
{
	Task<bool> WriteAsync(IEnumerable<TagHistory> records);
}