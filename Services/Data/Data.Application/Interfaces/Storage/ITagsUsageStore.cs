namespace Datalake.Data.Application.Interfaces.Storage;

public interface ITagsUsageStore
{
	void RegisterUsage(int tagId, string requestKey);

	IDictionary<string, DateTime>? Get(int tagId);
}
