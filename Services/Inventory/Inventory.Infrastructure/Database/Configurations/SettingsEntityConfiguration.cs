using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class SettingsEntityConfiguration : IEntityTypeConfiguration<SettingsEntity>
{
	public void Configure(EntityTypeBuilder<SettingsEntity> builder)
	{
		builder.HasNoKey();
	}
}
