using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database.Configurations.LinqToDb;

public class UserGroupConfiguration(DatabaseTableAccess access) : ILinqToDbEntityConfiguration<UserGroup>
{
	public void Configure(EntityMappingBuilder<UserGroup> builder)
	{
		builder
			.HasSchemaName(InventorySchema.Name)
			.HasTableName(InventorySchema.UserGroups.Name)
			.HasPrimaryKey(x => x.Guid);
	}
}
