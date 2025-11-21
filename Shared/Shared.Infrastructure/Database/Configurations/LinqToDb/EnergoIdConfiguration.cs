using Datalake.Domain.Entities;
using Datalake.Shared.Infrastructure.Database.Schema;
using LinqToDB.Mapping;

namespace Datalake.Shared.Infrastructure.Database.Configurations.LinqToDb;

public class EnergoIdConfiguration : ILinqToDbEntityConfiguration<EnergoId>
{
	public void Configure(EntityMappingBuilder<EnergoId> builder)
	{
		builder
			.HasSchemaName(EnergoIdSchema.Name)
			.HasTableName(EnergoIdSchema.EnergoId.Name)
			.HasPrimaryKey(x => x.Guid);
	}
}
