using Datalake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class SettingsConfiguration(bool isReadOnly = false) : IEntityTypeConfiguration<Settings>
{
	public void Configure(EntityTypeBuilder<Settings> builder)
	{
		builder.HasKey(x => x.Id);
	}
}
