using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Infrastructure.Repositories;

namespace Datalake.Data.Infrastructure.Database.Repositories;

[Scoped]
public class UserAccessRepository(DataDbContext context) : UserAccessAbstractRepository(context)
{
}
