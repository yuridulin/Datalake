using Datalake.Domain.Entities;
using Datalake.Inventory.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Inventory.Infrastructure.Database.Configurations;

public class EnergoIdEntityConfiguration : IEntityTypeConfiguration<EnergoId>
{
	public void Configure(EntityTypeBuilder<EnergoId> builder)
	{
		// представление для пользователей EnergoId
		builder.ToTable(name: null)
			.ToView(EnergoIdSchema.EnergoId.Name, EnergoIdSchema.Name);

		builder.HasKey(x => x.Guid);

		builder.HasOne(x => x.User)
			.WithOne(x => x.EnergoId)
			.HasForeignKey<User>(x => x.EnergoIdGuid)
			.HasPrincipalKey<EnergoId>(x => x.Guid);
	}
}
