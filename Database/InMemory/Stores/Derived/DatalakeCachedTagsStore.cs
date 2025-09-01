using Datalake.Database.Abstractions;
using Datalake.Database.InMemory.Models;
using Microsoft.Extensions.Logging;

namespace Datalake.Database.InMemory.Stores.Derived;

/// <summary>
/// Хранилище рассчитанных тегов
/// </summary>
public class DatalakeCachedTagsStore(
	DatalakeDataStore dataStore,
	ILogger<DatalakeCachedTagsStore> logger) : DatalakeDerivedStoreBase<DatalakeCachedTagsState>(dataStore, logger)
{
	/// <inheritdoc/>
	protected override DatalakeCachedTagsState CreateDerivedState(DatalakeDataState newDataState)
	{
		return new(newDataState);
	}

	/// <inheritdoc/>
	protected override string Type => nameof(DatalakeCachedTagsState);
}