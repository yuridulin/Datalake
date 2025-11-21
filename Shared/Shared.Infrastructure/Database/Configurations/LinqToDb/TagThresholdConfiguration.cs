using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database.Configurations.LinqToDb;

public class TagThresholdConfiguration(DatabaseTableAccess access) : ILinqToDbEntityConfiguration<TagThreshold>
{
	public void Configure(EntityMappingBuilder<TagThreshold> builder)
	{
		builder
			.HasSchemaName(InventorySchema.Name)
			.HasTableName(InventorySchema.TagThresholds.Name)
			.HasPrimaryKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
		{
			builder.HasIdentity(x => x.Id);
		}
	}
}
