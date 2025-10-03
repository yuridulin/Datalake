using Datalake.Shared.Application;
using Datalake.PublicApi.Models.Sources;
using System.Collections.Concurrent;
using Datalake.Data.Host.Abstractions;

namespace Datalake.Data.Host.Stores;

[Singleton]
public class SourcesStore(ILogger<SourcesStore> logger) : ISourcesStore
{
	private ConcurrentDictionary<int, SourceInfo> _sources = [];
	private readonly Lock _lock = new();

	public SourceInfo? TryGet(int id)
	{
		return _sources.TryGetValue(id, out var source) ? source : null;
	}

	public IReadOnlyCollection<SourceInfo> GetAll() => _sources.Values.ToArray();

	public void Update(IEnumerable<SourceInfo> newSources)
	{
		var sources = new ConcurrentDictionary<int, SourceInfo>(newSources.ToDictionary(x => x.Id));

		lock (_lock)
		{
			_sources = sources;
		}

		logger.LogInformation("Список источников данных обновлен");
	}
}