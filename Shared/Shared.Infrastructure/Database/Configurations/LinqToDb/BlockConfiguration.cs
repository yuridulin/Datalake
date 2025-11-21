using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database.Configurations.LinqToDb;

public class BlockConfiguration(DatabaseTableAccess access) : ILinqToDbEntityConfiguration<Block>
{
	public void Configure(EntityMappingBuilder<Block> builder)
	{
		builder
			.HasSchemaName(InventorySchema.Name)
			.HasTableName(InventorySchema.Blocks.Name)
			.HasPrimaryKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
		{
			builder.HasIdentity(x => x.Id);
		}
	}
}
