namespace Datalake.Data.Host;

public static class BootstrapExtensions
{
	public static IHostApplicationBuilder AddHosting(this IHostApplicationBuilder builder)
	{
		/*// сторы
		builder.Services.AddSingleton<IAccessStore, AccessStore>();
		builder.Services.AddSingleton<ITagsStore, TagsStore>();
		builder.Services.AddSingleton<ISourcesStore, SourcesStore>();
		builder.Services.AddSingleton<ICurrentValuesStore, CurrentValuesStore>();

		// сервисы
		builder.Services.AddSingleton<IAuthenticatorService, AuthenticationService>();
		builder.Services.AddSingleton<IReceiverService, ReceiverService>();
		builder.Services.AddSingleton<IGetValuesService, GetValuesService>();
		builder.Services.AddSingleton<IManualWriteValuesService, ManualWriteValuesService>();
		builder.Services.AddSingleton<ISystemWriteValuesService, SystemWriteValuesService>();

		builder.Services.AddSingleton<ITagHistoryFactory, TagHistoryFactory>();
		builder.Services.AddSingleton<ICollectorFactory, CollectorFactory>();

		builder.Services.AddSingleton<RequestsStateService>();
		builder.Services.AddSingleton<SourcesStateService>();
		builder.Services.AddSingleton<TagsReceiveStateService>();
		builder.Services.AddSingleton<TagsStateService>();

		builder.Services.AddScoped<IWriteHistoryRepository, WriteHistoryRepository>();
		builder.Services.AddScoped<IGetHistoryRepository, GetHistoryRepository>();
		builder.Services.AddScoped<IGetAggregatedHistoryRepository, GetAggregatedHistoryRepository>();

		// службы
		builder.Services.AddSingleton<ICollectorProcessor, CollectorProcessor>();
		builder.Services.AddSingleton<ICollectorWriter, CollectorWriter>();

		builder.Services.AddHostedService(provider => provider.GetRequiredService<ICollectorProcessor>());
		builder.Services.AddHostedService(provider => provider.GetRequiredService<ICollectorWriter>());*/

		return builder;
	}
}
