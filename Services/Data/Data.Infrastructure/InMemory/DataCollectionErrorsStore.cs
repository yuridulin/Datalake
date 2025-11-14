using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Values;
using Datalake.Domain.Extensions;
using Datalake.Shared.Application.Attributes;
using System.Collections.Concurrent;

namespace Datalake.Data.Infrastructure.InMemory;

[Singleton]
public class DataCollectionErrorsStore : IDataCollectionErrorsStore
{
	private ConcurrentDictionary<int, ValueCollectStatus> _state = [];

	public IEnumerable<ValueCollectStatus> Get(IEnumerable<int> identifiers)
	{
		List<ValueCollectStatus> statuses = new(identifiers.Count());

		foreach (var id in identifiers)
		{
			if (_state.TryGetValue(id, out var value))
				statuses.Add(value);
		}

		return statuses;
	}

	public void Set(int identifier, string? value)
	{
		var status = new ValueCollectStatus { Date = DateTimeExtension.GetCurrentDateTime(), HasError = string.IsNullOrEmpty(value), ErrorMessage = value };

		_state.AddOrUpdate(
			identifier,
			(id) => status,
			(id, existing) => status);
	}
}
