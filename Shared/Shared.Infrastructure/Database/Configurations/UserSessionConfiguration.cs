using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations;

public class UserSessionConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<UserSession>
{
	public void Configure(EntityTypeBuilder<UserSession> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(GatewaySchema.UserSessions.Name, GatewaySchema.Name);
		else
			builder.ToTable(GatewaySchema.UserSessions.Name, GatewaySchema.Name);

		// Настройка ключа
		builder.HasKey(u => u.Id);

		if (access == DatabaseTableAccess.Write)
			builder.Property(u => u.Id).ValueGeneratedOnAdd();

		var relationToUser = builder.HasOne(u => u.User)
			.WithMany(x => x.Sessions)
			.HasForeignKey(u => u.UserGuid)
			.HasPrincipalKey(x => x.Guid);

		if (access == DatabaseTableAccess.Write)
			relationToUser.OnDelete(DeleteBehavior.Cascade);

		// Настройка свойств
		builder
			.Property(u => u.Type)
			.HasConversion<string>()
			.HasMaxLength(20);
	}
}
