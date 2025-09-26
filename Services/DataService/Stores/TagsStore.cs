using Datalake.DataService.Abstractions;
using Datalake.PrivateApi.Attributes;
using Datalake.PublicApi.Models.Tags;
using System.Collections.Concurrent;

namespace Datalake.DataService.Stores;

[Singleton]
public class TagsStore(ILogger<TagsStore> logger) : ITagsStore
{
	private ConcurrentDictionary<int, TagCacheInfo> _tags = [];
	private ConcurrentDictionary<Guid, int> _mapToId = [];
	private readonly Lock _lock = new();

	public TagCacheInfo? TryGet(int id)
	{
		return _tags.TryGetValue(id, out var tag) ? tag : null;
	}

	public TagCacheInfo? TryGet(Guid guid)
	{
		if (!_mapToId.TryGetValue(guid, out var id))
			return null;
		
		return _tags.TryGetValue(id, out var tag) ? tag : null;
	}

	public void Update(IEnumerable<TagCacheInfo> newTags)
	{
		var tags = new ConcurrentDictionary<int, TagCacheInfo>(newTags.ToDictionary(x => x.Id));
		var mapping = new ConcurrentDictionary<Guid, int>(newTags.ToDictionary(x => x.Guid, x => x.Id));

		lock (_lock)
		{
			_tags = tags;
			_mapToId = mapping;
		}

		logger.LogInformation("Список тегов обновлен");
	}

	public IReadOnlyCollection<TagCacheInfo> GetBySourceId(int sourceId)
	{
		return _tags.Values.Where(x => x.SourceId == sourceId).ToArray();
	}
}