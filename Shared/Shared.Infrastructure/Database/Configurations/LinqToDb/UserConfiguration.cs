using Datalake.Domain.Entities;
using Datalake.Domain.Enums;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure.Database.Schema;
using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database.Configurations.LinqToDb;

public class UserConfiguration(DatabaseTableAccess access) : ILinqToDbEntityConfiguration<User>
{
	public void Configure(EntityMappingBuilder<User> builder)
	{
		builder
			.HasSchemaName(InventorySchema.Name)
			.HasTableName(InventorySchema.Users.Name)
			.HasPrimaryKey(x => x.Guid);

		// Настройка преобразования PasswordHash
		builder
			.Property(x => x.PasswordHash)
			.HasColumnName("PasswordHash")
			.HasConversion(
				password => password != null ? password.ToString() : null,
				hash => string.IsNullOrEmpty(hash) ? null : PasswordHashValue.FromExistingHash(hash));

		// Настройка преобразования Type
		builder
			.Property(x => x.Type)
			.HasConversion(x => x.ToString(), x => Enum.Parse<UserType>(x));
	}
}
