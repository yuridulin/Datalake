using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Configurations;

public class UserConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		if (isReadOnly)
			builder.ToView(InventorySchema.Users.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Users.Name, InventorySchema.Name);

		// Настройка ключа
		builder.HasKey(u => u.Guid);

		// Настройка индексов
		builder.HasIndex(u => new { u.Login, u.EnergoIdGuid }).IsUnique();
		builder.HasIndex(u => u.Type); 

		// Настройка свойств
		builder
			.Property(u => u.FullName)
			.IsRequired()
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
