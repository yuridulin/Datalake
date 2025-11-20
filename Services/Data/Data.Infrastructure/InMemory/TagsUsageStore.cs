using Datalake.Data.Application.Interfaces.Storage;
using Datalake.Shared.Application.Attributes;
using System.Collections.Concurrent;

namespace Datalake.Data.Infrastructure.InMemory;

[Singleton]
public class TagsUsageStore : ITagsUsageStore
{
	private readonly ConcurrentDictionary<int, TagUsage> _usage = new();

	public void RegisterUsage(int tagId, string requestKey)
	{
		var info = _usage.GetOrAdd(tagId, _ => new TagUsage(tagId));
		info.Update(requestKey);
	}

	public IDictionary<string, DateTime>? GetUsage(int tagId)
	{
		_usage.TryGetValue(tagId, out var info);
		return info?.RequestsUsage;
	}
}

public record TagUsage
{
	public TagUsage(int tagId)
	{
		TagId = tagId;
		RequestsUsage = [];
	}

	public TagUsage(int tagId, string requestKey) : this(tagId)
	{
		Update(requestKey);
	}

	public void Update(string requestKey)
	{
		RequestsUsage[requestKey] = DateTime.UtcNow;
	}

	public int TagId { get; }

	public Dictionary<string, DateTime> RequestsUsage { get; }
}
