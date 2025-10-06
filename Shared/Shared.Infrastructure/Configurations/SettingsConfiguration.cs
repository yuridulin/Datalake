using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Configurations;

public class SettingsConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<Settings>
{
	public void Configure(EntityTypeBuilder<Settings> builder)
	{
		if (isReadOnly)
			builder.ToView(InventorySchema.Settings.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Settings.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (!isReadOnly)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();
	}
}
