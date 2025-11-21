using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure.Database.Schema;
using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database.Configurations.LinqToDb;

public class CalculatedAccessRuleConfiguration(DatabaseTableAccess access) : ILinqToDbEntityConfiguration<CalculatedAccessRule>
{
	public void Configure(EntityMappingBuilder<CalculatedAccessRule> builder)
	{
		builder
			.HasSchemaName(InventorySchema.Name)
			.HasTableName(InventorySchema.CalculatedAccessRules.Name)
			.HasPrimaryKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
		{
			builder.HasIdentity(x => x.Id);
		}
	}
}
