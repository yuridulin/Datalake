namespace Datalake.Data.Application.Interfaces.Cache;

public interface ITagsUsageStore
{
	void RegisterUsage(int tagId, string requestKey);

	IDictionary<string, DateTime>? GetUsage(int tagId);
}
