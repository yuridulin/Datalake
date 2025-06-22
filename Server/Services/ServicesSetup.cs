using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Repositories;
using Datalake.Server.BackgroundServices.Collector;
using Datalake.Server.BackgroundServices.HistoryIndexer;
using Datalake.Server.BackgroundServices.HistoryInitial;
using Datalake.Server.BackgroundServices.SettingsHandler;
using Datalake.Server.Services.Receiver;
using Datalake.Server.Services.SessionManager;
using Datalake.Server.Services.StateManager;

namespace Datalake.Server.Services;

internal static class ServicesSetup
{
	internal static void AddServices(this WebApplicationBuilder builder)
	{
		// постоянные
		builder.Services.AddSingleton<CollectorFactory>();
		builder.Services.AddSingleton<ReceiverService>();
		builder.Services.AddSingleton<SessionManagerService>();
		builder.Services.AddSingleton<SettingsHandlerService>();
		builder.Services.AddSingleton<ISettingsUpdater>(provider
			=> provider.GetRequiredService<SettingsHandlerService>());
		builder.Services.AddSingleton<SourcesStateService>();
		builder.Services.AddSingleton<UsersStateService>();
		builder.Services.AddSingleton<TagsStateService>();

		builder.Services.AddSingleton<DatalakeStateHolder>(); // стейт-менеджер с инициализацией
		builder.Services.AddSingleton<InMemoryRepositoriesManager>(); // репозитории
		builder.Services.AddSingleton<DerivedDataStore>(); // стейт-менеджер зависимых данных

		// службы
		builder.Services.AddHostedService<CollectorProcessor>();
		builder.Services.AddHostedService<CollectorWriter>();
		builder.Services.AddHostedService<HistoryIndexerService>();
		builder.Services.AddHostedService<HistoryInitialService>();
		builder.Services.AddHostedService<SettingsHandlerService>();
		builder.Services.AddHostedService(provider
			=> provider.GetRequiredService<SettingsHandlerService>());
	}
}
