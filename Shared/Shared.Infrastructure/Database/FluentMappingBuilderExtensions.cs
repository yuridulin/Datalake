using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database;

/// <summary>
/// Расширения для FluentMappingBuilder для применения конфигураций
/// </summary>
public static class FluentMappingBuilderExtensions
{
	/// <summary>
	/// Применить конфигурацию к построителю маппинга
	/// </summary>
	public static FluentMappingBuilder ApplyConfiguration<T>(
		this FluentMappingBuilder builder,
		ILinqToDbEntityConfiguration<T> configuration)
	{
		var entityBuilder = builder.Entity<T>();
		// LinqToDB используется только для чтения, но конфигурация может использовать access для определения схемы
		configuration.Configure(entityBuilder);
		entityBuilder.Build();

		return builder;
	}
}
