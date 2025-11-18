using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Values;
using Datalake.Shared.Application.Attributes;
using System.Collections.Concurrent;

namespace Datalake.Data.Infrastructure.InMemory;

[Singleton]
public class TagsCollectionStatusStore : ITagsCollectionStatusStore
{
	private ConcurrentDictionary<int, TagCollectionStatus> _state = [];

	public IEnumerable<TagCollectionStatus> Get(IEnumerable<int> identifiers)
	{
		List<TagCollectionStatus> statuses = new(identifiers.Count());

		foreach (var id in identifiers)
		{
			if (_state.TryGetValue(id, out var value))
				statuses.Add(value);
		}

		return statuses;
	}

	public void Set(int identifier, string? value)
	{
		var status = new TagCollectionStatus { Date = DateTime.UtcNow, HasError = string.IsNullOrEmpty(value), ErrorMessage = value };

		_state.AddOrUpdate(
			identifier,
			(id) => status,
			(id, existing) => status);
	}
}
