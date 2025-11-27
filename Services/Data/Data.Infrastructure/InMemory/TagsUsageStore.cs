using Datalake.Data.Application.Interfaces.Storage;
using Datalake.Shared.Application.Attributes;
using System.Collections.Concurrent;

namespace Datalake.Data.Infrastructure.InMemory;

[Singleton]
public class TagsUsageStore : ITagsUsageStore
{
	private readonly ConcurrentDictionary<int, TagUsage> usage = new();

	public void RegisterUsage(int tagId, string requestKey)
	{
		var info = usage.GetOrAdd(tagId, _ => new TagUsage(tagId));
		info.Update(requestKey);
	}

	public IDictionary<string, DateTime>? Get(int tagId)
	{
		usage.TryGetValue(tagId, out var info);
		return info?.RequestsUsage;
	}
}

public class TagUsage(int tagId)
{
	private readonly ConcurrentDictionary<string, DateTime> usage = [];

	public TagUsage(int tagId, string requestKey) : this(tagId)
	{
		Update(requestKey);
	}

	public void Update(string requestKey)
	{
		usage.AddOrUpdate(requestKey, (_) => DateTime.UtcNow, (_, _) => DateTime.UtcNow);
	}

	public int TagId { get; } = tagId;

	public IDictionary<string, DateTime> RequestsUsage => usage;
}
