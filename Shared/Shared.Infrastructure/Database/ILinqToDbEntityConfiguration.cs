using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database;

/// <summary>
/// Интерфейс для конфигурации маппинга сущности LinqToDB
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
public interface ILinqToDbEntityConfiguration<T>
{
	/// <summary>
	/// Настраивает маппинг сущности
	/// </summary>
	/// <param name="builder">Построитель маппинга</param>
	/// <param name="access">Уровень доступа к таблице</param>
	void Configure(EntityMappingBuilder<T> builder);
}
