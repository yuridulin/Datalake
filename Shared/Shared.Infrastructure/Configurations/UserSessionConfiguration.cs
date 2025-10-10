using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

namespace Datalake.Shared.Infrastructure.Configurations;

public class UserSessionConfiguration(TableAccess access) : IEntityTypeConfiguration<UserSession>
{
	public void Configure(EntityTypeBuilder<UserSession> builder)
	{
		if (access == TableAccess.Read)
			builder.ToView(GatewaySchema.UserSessions.Name, GatewaySchema.Name);
		else
			builder.ToTable(GatewaySchema.UserSessions.Name, GatewaySchema.Name);

		// Настройка ключа
		builder.HasKey(u => u.Id);

		if (access == TableAccess.Write)
			builder.Property(u => u.Id).ValueGeneratedOnAdd();

		var relationToUser = builder.HasOne(u => u.User)
			.WithMany(x => x.Sessions)
			.HasForeignKey(u => u.UserGuid)
			.HasPrincipalKey(x => x.Guid);

		if (access == TableAccess.Write)
			relationToUser.OnDelete(DeleteBehavior.Cascade);

		// Настройка свойств
		builder
			.Property(u => u.Type)
			.HasConversion<string>()
			.HasMaxLength(20);

		builder
			.Property(x => x.Token)
			.HasConversion(
				token => token.ToString(),
				rawHash => PasswordHashValue.FromExistingHash(rawHash))
			.HasColumnName(GatewaySchema.UserSessions.Columns.Token);
	}
}
