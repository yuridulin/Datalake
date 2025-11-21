using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database.Configurations.LinqToDb;

public class TagConfiguration(DatabaseTableAccess access) : ILinqToDbEntityConfiguration<Tag>
{
	public void Configure(EntityMappingBuilder<Tag> builder)
	{
		builder
			.HasSchemaName(InventorySchema.Name)
			.HasTableName(InventorySchema.Tags.Name)
			.HasPrimaryKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
		{
			builder.HasIdentity(x => x.Id);
		}
	}
}
