using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Infrastructure.Database.Repositories;

namespace Datalake.Data.Infrastructure.Database.Repositories;

[Scoped]
public class UserAccessValuesRepository(DataDbContext context) : AbstractUserAccessValuesRepository(context)
{
}
