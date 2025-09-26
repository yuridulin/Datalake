using Datalake.Database.Abstractions;
using Datalake.Database.InMemory.Models;
using Datalake.Database.InMemory.Stores;
using Datalake.Inventory.Functions;
using Datalake.Inventory.InMemory.Models;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.InMemory.Stores.Derived;

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