using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Datalake.Shared.Infrastructure.ConfigurationsApplyHelper;

namespace Datalake.Shared.Infrastructure.Configurations;

public class UserConfiguration(TableAccess access) : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		if (access == TableAccess.Read)
			builder.ToView(InventorySchema.Users.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Users.Name, InventorySchema.Name);

		// Настройка ключа
		builder.HasKey(u => u.Guid);

		// Настройка индексов
		if (access == TableAccess.Write)
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
