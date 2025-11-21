using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Infrastructure.Database;
using LinqToDB;
using LinqToDB.Mapping;

namespace Datalake.Data.Infrastructure.Database;

/// <summary>
/// Контекст LinqToDB для работы с данными тегов
/// </summary>
[Scoped]
public class DataDbLinqContext(DataOptions<DataDbLinqContext> options) : AbstractLinqToDbContext(
	options.Options,
	CreateMappingSchema())
{
	private static MappingSchema CreateMappingSchema()
	{
		var mappings = new MappingSchema();
		var builder = new FluentMappingBuilder(mappings);

		ApplyConfigurations(builder, TableAccessConfiguration);

		return mappings;
	}

	private static DatabaseTableAccessConfiguration TableAccessConfiguration { get; } = new()
	{
		AccessRules = DatabaseTableAccess.Read,
		Audit = DatabaseTableAccess.Read,
		Blocks = DatabaseTableAccess.Read,
		BlocksProperties = DatabaseTableAccess.Read,
		BlocksTags = DatabaseTableAccess.Read,
		CalculatedAccessRules = DatabaseTableAccess.Read,
		Settings = DatabaseTableAccess.Read,
		Sources = DatabaseTableAccess.Read,
		Tags = DatabaseTableAccess.Read,
		TagsValues = DatabaseTableAccess.Write,
		TagsInputs = DatabaseTableAccess.Read,
		TagsThresholds = DatabaseTableAccess.Read,
		UserGroups = DatabaseTableAccess.Read,
		UserGroupsRelations = DatabaseTableAccess.Read,
		Users = DatabaseTableAccess.Read,
		UserSessions = DatabaseTableAccess.Read,
	};
}
