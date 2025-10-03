using Datalake.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Data.Infrastructure.Database.Configurations;

internal class TagHistoryConfiguration : IEntityTypeConfiguration<TagHistoryValue>
{
	public void Configure(EntityTypeBuilder<TagHistoryValue> builder)
	{
		// Настройка таблицы
		builder.ToTable("TagsHistory");
	}
}