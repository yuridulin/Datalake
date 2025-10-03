using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
	public void Configure(EntityTypeBuilder<UserEntity> builder)
	{
		// Настройка таблицы
		builder.ToTable("Users");

		// Настройка ключа
		builder.HasKey(u => u.Guid);

		// Настройка индексов
		builder.HasIndex(u => u.Login).IsUnique();
		builder.HasIndex(u => u.EnergoIdGuid).IsUnique();
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
				hash => hash != null ? PasswordHashValue.FromExistingHash(hash) : null)
			.HasColumnName("PasswordHash");

		// Настройка отношений "многие-ко-многим" с группами
		builder
			.HasMany(u => u.Groups)
			.WithMany(g => g.Users)
			.UsingEntity<UserGroupRelationEntity>(
				j => j.HasOne(ug => ug.UserGroup).WithMany().HasForeignKey(ug => ug.UserGroupGuid),
				j => j.HasOne(ug => ug.User).WithMany().HasForeignKey(ug => ug.UserGuid)
			);
	}
}
