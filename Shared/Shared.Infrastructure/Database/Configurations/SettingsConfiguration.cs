using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations;

public class SettingsConfiguration(DatabaseTableAccess access) : IEntityTypeConfiguration<Settings>
{
	public void Configure(EntityTypeBuilder<Settings> builder)
	{
		if (access == DatabaseTableAccess.Read)
			builder.ToView(InventorySchema.Settings.Name, InventorySchema.Name);
		else
			builder.ToTable(InventorySchema.Settings.Name, InventorySchema.Name);

		builder.HasKey(x => x.Id);

		if (access == DatabaseTableAccess.Write)
			builder.Property(x => x.Id).ValueGeneratedOnAdd();
	}
}
