using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Database.Configurations.EntityFramework;

public class EnergoIdConfiguration : IEntityTypeConfiguration<EnergoId>
{
	public void Configure(EntityTypeBuilder<EnergoId> builder)
	{
		// представление для пользователей EnergoId
		builder.ToView(EnergoIdSchema.EnergoId.Name, EnergoIdSchema.Name);

		builder.HasKey(x => x.Guid);

		builder.HasOne(x => x.User)
			.WithOne(x => x.EnergoId)
			.HasForeignKey<User>(x => x.Guid)
			.HasPrincipalKey<EnergoId>(x => x.Guid);
	}
}
