using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Configurations;

public class UserSessionConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<UserSession>
{
	public void Configure(EntityTypeBuilder<UserSession> builder)
	{
		if (isReadOnly)
			builder.ToView(InventorySchema.UserSessions.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.UserSessions.Name, InventorySchema.Name);

		// Настройка ключа
		builder.HasKey(u => u.Id);

		if (!isReadOnly)
			builder.Property(u => u.Id).ValueGeneratedOnAdd();

		var relationToUser = builder.HasOne(u => u.User)
			.WithMany()
			.HasForeignKey(u => u.UserGuid);

		if (!isReadOnly)
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
			.HasColumnName("Token");
	}
}
