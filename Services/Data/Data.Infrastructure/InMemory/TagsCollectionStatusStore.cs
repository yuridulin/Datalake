using Datalake.Contracts.Models.Tags;
using Datalake.Data.Application.Interfaces.Storage;
using Datalake.Shared.Application.Attributes;
using System.Collections.Concurrent;

namespace Datalake.Data.Infrastructure.InMemory;

[Singleton]
public class TagsCollectionStatusStore : ITagsCollectionStatusStore
{
	private ConcurrentDictionary<int, TagStatusInfo> _state = [];

	public IEnumerable<TagStatusInfo> Get(IEnumerable<int> identifiers)
	{
		List<TagStatusInfo> statuses = new(identifiers.Count());

		foreach (var id in identifiers)
		{
			if (_state.TryGetValue(id, out var value))
				statuses.Add(value);
		}

		return statuses;
	}

	public void Set(int identifier, string? value)
	{
		var status = new TagStatusInfo(
			identifier,
			DateTime.UtcNow,
			!string.IsNullOrEmpty(value),
			value);

		_state.AddOrUpdate(
			identifier,
			(id) => status,
			(id, existing) => status);
	}
}
