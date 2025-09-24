using Datalake.Database.Abstractions;
using Datalake.Database.Functions;
using Datalake.Database.InMemory.Models;
using Microsoft.Extensions.Logging;

namespace Datalake.Database.InMemory.Stores.Derived;

/// <summary>
/// Хранилище рассчитанных прав доступа данных
/// </summary>
public class DatalakeAccessStore(
	DatalakeDataStore dataStore,
	ILogger<DatalakeAccessStore> logger) : DatalakeDerivedStoreBase<DatalakeAccessState>(dataStore, logger)
{
	/// <inheritdoc/>
	protected override DatalakeAccessState CreateDerivedState(DatalakeDataState newDataState)
	{
		return AccessFunctions.ComputeAccess(newDataState);
	}

	/// <inheritdoc/>
	protected override string Type => nameof(DatalakeAccessState);
}