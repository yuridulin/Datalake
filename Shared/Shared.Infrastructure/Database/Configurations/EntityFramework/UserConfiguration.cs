using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations.EntityFramework;

public class UserConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(InventorySchema.Users.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Users.Name, InventorySchema.Name);

		// Настройка ключа
		builder.HasKey(u => u.Guid);

		// Настройка индексов
		if (access == DatabaseTableAccess.Write)
		{
			builder.HasIndex(u => u.Type);
		}

		// Настройка свойств
		builder
			.Property(u => u.FullName)
			.HasMaxLength(200);

		builder
			.Property(u => u.Login)
			.HasMaxLength(100);

		builder
			.Property(u => u.Type)
			.HasConversion<string>()
			.HasMaxLength(20);

		builder
			.Property(x => x.PasswordHash)
			.HasConversion(
				password => password != null ? password.ToString() : null,
				hash => string.IsNullOrEmpty(hash) ? null : PasswordHashValue.FromExistingHash(hash))
			.HasColumnName("PasswordHash");
	}
}
