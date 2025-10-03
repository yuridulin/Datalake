using Datalake.DataService.Database.Entities;

namespace Datalake.DataService.Database.Interfaces;

public interface IWriteHistoryRepository
{
	Task<bool> WriteAsync(IEnumerable<TagHistory> records);
}