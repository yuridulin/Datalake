using Datalake.Domain.Entities;
using Datalake.Domain.Enums;
using Datalake.Shared.Infrastructure.Database.Schema;
using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database.Configurations.LinqToDb;

public class UserSessionConfiguration(DatabaseTableAccess access) : ILinqToDbEntityConfiguration<UserSession>
{
	public void Configure(EntityMappingBuilder<UserSession> builder)
	{
		builder
			.HasSchemaName(GatewaySchema.Name)
			.HasTableName(GatewaySchema.UserSessions.Name)
			.HasPrimaryKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
		{
			builder.HasIdentity(x => x.Id);
		}

		// Настройка преобразования Type
		builder
			.Property(x => x.Type)
			.HasConversion(x => x.ToString(), x => Enum.Parse<UserType>(x));
	}
}
