using Datalake.Data.Host.Database.Entities;

namespace Datalake.Data.Host.Database.Interfaces;

public interface IGetHistoryRepository
{
	Task<IEnumerable<TagHistory>> GetLastValuesAsync();

	Task<IEnumerable<TagHistory>> GetLastValuesAsync(int[] tagsId);

	Task<IEnumerable<TagHistory>> GetExactValuesAsync(DateTime exactDate, int[] tagsId);

	Task<IEnumerable<TagHistory>> GetRangeValuesAsync(DateTime fromDate, DateTime toDate, int[] tagsId);
}

