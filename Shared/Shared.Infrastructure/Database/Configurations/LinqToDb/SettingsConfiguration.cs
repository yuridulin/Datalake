using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database.Configurations.LinqToDb;

public class SettingsConfiguration(DatabaseTableAccess access) : ILinqToDbEntityConfiguration<Settings>
{
	public void Configure(EntityMappingBuilder<Settings> builder)
	{
		builder
			.HasSchemaName(InventorySchema.Name)
			.HasTableName(InventorySchema.Settings.Name)
			.HasPrimaryKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
		{
			builder.HasIdentity(x => x.Id);
		}
	}
}
