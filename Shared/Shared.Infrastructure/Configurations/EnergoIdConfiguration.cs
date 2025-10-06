using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datalake.Shared.Infrastructure.Configurations;

public class EnergoIdConfiguration : IEntityTypeConfiguration<EnergoId>
{
	public void Configure(EntityTypeBuilder<EnergoId> builder)
	{
		// представление для пользователей EnergoId
		builder.ToView(EnergoIdSchema.EnergoId.Name, EnergoIdSchema.Name);

		builder.HasKey(x => x.Guid);

		builder.HasOne(x => x.User)
			.WithOne(x => x.EnergoId)
			.HasForeignKey<User>(x => x.EnergoIdGuid)
			.HasPrincipalKey<EnergoId>(x => x.Guid);
	}
}
