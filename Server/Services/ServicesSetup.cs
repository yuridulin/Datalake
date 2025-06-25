using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Repositories;
using Datalake.Database.Repositories;
using Datalake.Server.Services.Auth;
using Datalake.Server.Services.Collection;
using Datalake.Server.Services.Receiver;
using Datalake.Server.Services.SettingsHandler;
using Datalake.Server.Services.StateManager;

namespace Datalake.Server.Services;

internal static class ServicesSetup
{
	internal static void AddServices(this WebApplicationBuilder builder)
	{
		// хранилища данный
		builder.Services.AddSingleton<DatalakeDataStore>(); // стейт-менеджер исходных данных
		builder.Services.AddSingleton<DatalakeDerivedDataStore>(); // стейт-менеджер зависимых данных
		builder.Services.AddSingleton<DatalakeCurrentValuesStore>(); // кэш последних значений

		// репозитории в памяти
		builder.Services.AddScoped<SettingsMemoryRepository>();
		builder.Services.AddScoped<AccessRightsMemoryRepository>();
		builder.Services.AddScoped<BlocksMemoryRepository>();
		builder.Services.AddScoped<SourcesMemoryRepository>();
		builder.Services.AddScoped<TagsMemoryRepository>();
		builder.Services.AddScoped<UserGroupsMemoryRepository>();
		builder.Services.AddScoped<UsersMemoryRepository>();

		// репозитории только БД
		builder.Services.AddScoped<AuditRepository>();
		builder.Services.AddScoped<ValuesRepository>();

		// сервис получения данных
		builder.Services.AddSingleton<ReceiverService>();

		// мониторинг активности
		builder.Services.AddSingleton<SessionManagerService>();
		builder.Services.AddSingleton<SourcesStateService>();
		builder.Services.AddSingleton<UsersStateService>();
		builder.Services.AddSingleton<TagsStateService>();

		// система сбора данных
		builder.Services.AddSingleton<CollectorWriter>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<CollectorWriter>());
		builder.Services.AddHostedService<CollectorProcessor>();
		builder.Services.AddSingleton<CollectorFactory>();

		// работа с пользователями
		builder.Services.AddScoped<AuthenticationService>();

		// обновление настроек
		builder.Services.AddSingleton<SettingsHandlerService>();
		builder.Services.AddHostedService<SettingsHandlerService>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<SettingsHandlerService>());
	}
}
