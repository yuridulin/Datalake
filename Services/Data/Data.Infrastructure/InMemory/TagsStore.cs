using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Data.Application.Models.Tags;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Datalake.Data.Infrastructure.InMemory;

[Singleton]
public class TagsStore(ILogger<TagsStore> logger) : ITagsStore
{
	private IEnumerable<TagSettingsDto> _tags = [];
	private ConcurrentDictionary<int, TagSettingsDto> _tagsById = [];
	private ConcurrentDictionary<Guid, TagSettingsDto> _tagsByGuid = [];
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public TagSettingsDto? TryGet(int id)
	{
		return _tagsById.TryGetValue(id, out var tag) ? tag : null;
	}

	public TagSettingsDto? TryGet(Guid guid)
	{
		return _tagsByGuid.TryGetValue(guid, out var tag) ? tag : null;
	}

	public async Task UpdateAsync(IEnumerable<TagSettingsDto> newTags)
	{
		await _semaphore.WaitAsync();

		try
		{
			Interlocked.Exchange(ref _tags, newTags.ToImmutableArray());

			var tagsById = new ConcurrentDictionary<int, TagSettingsDto>(_tags.ToDictionary(x => x.TagId));
			var tagsByGuid = new ConcurrentDictionary<Guid, TagSettingsDto>(_tags.ToDictionary(x => x.TagGuid));

			Interlocked.Exchange(ref _tagsById, tagsById);
			Interlocked.Exchange(ref _tagsByGuid, tagsByGuid);

			logger.LogInformation("Список тегов обновлен");
		}
		finally
		{
			_semaphore.Release();
		}
	}
}
